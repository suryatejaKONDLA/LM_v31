import { useState, useCallback, useEffect, useRef, useMemo } from "react";
import { useForm, type FieldValues, type DefaultValues, type UseFormReturn } from "react-hook-form";
import { GlobalSpinner, ModalDialog, type ResetConfirmProps, ApiResponseCode, type ApiResponseWithData, type ApiValidationResponse,isCancelledRequest } from "@/Shared/Index";
import { useQueryParams } from "./useQueryParams";
import { useUnsavedChanges } from "./useUnsavedChanges";
import { useFormShortcuts } from "./useKeyboardShortcuts";
import { useBranchStore } from "../Stores/BranchStore";

// ─── Types ─────────────────────────────────────────────────────

type BannerFields = [string | null, string | null, string | null];

export interface MasterFormConfig<TFormValues extends FieldValues, TResponse>
{
    /** Initial empty form values for add mode. */
    defaultValues: TFormValues;

    /** Page title — set as document.title. */
    pageTitle: string;

    /** Fetch a single record by numeric ID. */
    fetchById: (id: number, signal?: AbortSignal) => Promise<ApiResponseWithData<TResponse>>;

    /** Create or update a record. */
    save: (request: unknown) => Promise<ApiResponseWithData<null>>;

    /** Map backend response → form field values (edit mode). */
    mapResponseToForm: (data: TResponse) => TFormValues;

    /** Map form values → API request body. */
    buildRequest: (values: TFormValues, qString1: string, branchCode: number) => unknown;

    /** Build [RecNo, Created, Modified] banner fields from response. */
    buildBannerFields: (data: TResponse) => BannerFields;

    /** Custom validation before submit. Return error message or null. */
    validateBeforeSubmit?: (values: TFormValues) => string | null;

    /** Callback after a successful save. */
    onSaveSuccess?: (isEditMode: boolean) => void;
}

export interface MasterFormReturn<TFormValues extends FieldValues>
{
    form: UseFormReturn<TFormValues>;
    submitting: boolean;
    isDirty: boolean;
    isEditMode: boolean;
    qString1: string;
    qString2: string;
    qString3: string;
    bannerFields: BannerFields;
    handleFormSubmit: () => void;
    onReset: () => void;
    resetConfirmProps: ResetConfirmProps | undefined;
}

// ─── Hook ──────────────────────────────────────────────────────

/**
 * Generic hook for master form pages (RoleMaster, LoginMaster, BranchMaster, etc.).
 *
 * Handles query-string parsing, fetch-by-id in edit mode, submit,
 * reset with confirmation dialogs, keyboard shortcuts (Ctrl+S / Ctrl+R),
 * and browser-close warning when dirty.
 *
 * **Reset behaviour (mirrors old app):**
 * - Add mode (qString1 = "0"): confirms if dirty, then clears form to defaults.
 * - Edit mode (qString1 ≠ "0"): always confirms "Reload from DB?", then re-fetches.
 */
export function useMasterForm<TFormValues extends FieldValues, TResponse>(
    config: MasterFormConfig<TFormValues, TResponse>,
): MasterFormReturn<TFormValues>
{
    // Stable ref for the entire config — prevents infinite re-renders
    // when consumers pass inline objects/callbacks.
    const configRef = useRef(config);
    configRef.current = config;

    // Stable ref for defaultValues — avoids object identity issues in deps.
    const defaultsRef = useRef(config.defaultValues);
    defaultsRef.current = config.defaultValues;

    // ─── Query Params ──────────────────────────────────────────
    const { qString1, qString2, qString3, isEditMode } = useQueryParams();

    // ─── Branch Code (direct store access, no dynamic import) ──
    const branchCode = useBranchStore((s) => s.activeBranch?.BRANCH_Code ?? 0);
    const branchCodeRef = useRef(branchCode);
    branchCodeRef.current = branchCode;

    // ─── Page Title ────────────────────────────────────────────
    useEffect(() =>
    {
        document.title = configRef.current.pageTitle;
    }, [ qString1 ]);

    // ─── State ─────────────────────────────────────────────────
    const [ submitting, setSubmitting ] = useState(false);
    const [ bannerFields, setBannerFields ] = useState<BannerFields>([ null, null, null ]);

    // ─── Form Setup ────────────────────────────────────────────
    const form = useForm<TFormValues>({
        defaultValues: config.defaultValues as DefaultValues<TFormValues>,
        mode: "onBlur",
    });

    // Read isDirty from the proxy on every render — not destructured once.
    const { handleSubmit, reset } = form;
    const isDirty = form.formState.isDirty;

    // Warn on browser close / tab close when dirty
    useUnsavedChanges(isDirty, config.pageTitle);

    // ─── Reset to Defaults (Add Mode) ──────────────────────────
    const resetToDefault = useCallback(() =>
    {
        reset(defaultsRef.current as DefaultValues<TFormValues>);
        setBannerFields([ null, null, null ]);
    }, [ reset ]);

    // ─── Fetch Record (Edit Mode) ─────────────────────────────
    // Uses a ref for qString1 so the function identity stays stable
    // and manual calls (from confirmReload) don't get aborted by the
    // initial-load effect's cleanup.
    const qString1Ref = useRef(qString1);
    qString1Ref.current = qString1;

    const fetchRecord = useCallback(async (signal?: AbortSignal): Promise<void> =>
    {
        GlobalSpinner.show("Loading…");
        try
        {
            const result = await configRef.current.fetchById(Number(qString1Ref.current), signal);

            if (result.Code !== ApiResponseCode.Success)
            {
                ModalDialog.warning({
                    title: "Record Not Found",
                    content: "The requested record could not be found.",
                    onOk: () =>
                    {
                        resetToDefault();
                    },
                });
                return;
            }

            const data = result.Data;
            const formValues = configRef.current.mapResponseToForm(data);
            reset(formValues as DefaultValues<TFormValues>);
            setBannerFields(configRef.current.buildBannerFields(data));
        }
        catch (err: unknown)
        {
            if (isCancelledRequest(err))
            {
                return;
            }
            ModalDialog.error({
                title: "Error",
                content: "Failed to load record. Please try again.",
            });
        }
        finally
        {
            GlobalSpinner.hide();
        }
    }, [ reset, resetToDefault ]);

    // ─── Initial Load ──────────────────────────────────────────
    useEffect(() =>
    {
        if (!isEditMode)
        {
            resetToDefault();
            return;
        }

        const controller = new AbortController();
        void fetchRecord(controller.signal);

        return () =>
        {
            controller.abort();
        };
    }, [ qString1, isEditMode, fetchRecord, resetToDefault ]);

    // ─── Submit ────────────────────────────────────────────────
    const handleFormSubmit = useCallback(() =>
    {
        const onSubmit = async (formValues: TFormValues): Promise<void> =>
        {
            // Custom pre-submit validation
            if (configRef.current.validateBeforeSubmit)
            {
                const error = configRef.current.validateBeforeSubmit(formValues);
                if (error)
                {
                    ModalDialog.showResult("warning", error);
                    return;
                }
            }

            setSubmitting(true);
            GlobalSpinner.show("Saving…");

            try
            {
                const request = configRef.current.buildRequest(
                    formValues,
                    qString1Ref.current,
                    branchCodeRef.current,
                );

                const result = await configRef.current.save(request);

                if (result.Code === ApiResponseCode.Success)
                {
                    // Show success — reset/re-fetch happens inside onOk
                    // so the user sees the dialog before the form changes.
                    ModalDialog.successResult(result.Message || "Saved successfully", () =>
                    {
                        const editMode = qString1Ref.current !== "0" && qString1Ref.current !== "";

                        if (editMode)
                        {
                            void fetchRecord();
                        }
                        else
                        {
                            resetToDefault();
                        }

                        configRef.current.onSaveSuccess?.(editMode);
                    });
                }
                else
                {
                    // Check for field-level validation errors
                    const validationResponse = result as unknown as ApiValidationResponse;
                    if (validationResponse.Errors.length > 0)
                    {
                        ModalDialog.showValidationErrors(validationResponse.Errors);
                    }
                    else
                    {
                        ModalDialog.showResult(result.Type, result.Message);
                    }
                }
            }
            catch
            {
                ModalDialog.showResult("error", "An unexpected error occurred. Please try again.");
            }
            finally
            {
                setSubmitting(false);
                GlobalSpinner.hide();
            }
        };

        void handleSubmit(onSubmit)();
    }, [ handleSubmit, fetchRecord, resetToDefault ]);

    // ─── Reset ─────────────────────────────────────────────────
    const onReset = useCallback(() =>
    {
        if (isEditMode)
        {
            void fetchRecord();
        }
        else
        {
            resetToDefault();
        }
    }, [ isEditMode, fetchRecord, resetToDefault ]);

    // ─── Popconfirm Props for Reset Button ─────────────────────
    // Returns props for the Popconfirm wrapper.
    // - Edit mode: always show "Reload from DB?"
    // - Add mode + dirty: show "Reset form?"
    // - Add mode + clean: no Popconfirm needed (undefined)
    const resetConfirmProps = useMemo<ResetConfirmProps | undefined>(() =>
    {
        if (isEditMode)
        {
            return {
                title: "Reload Record",
                description: "Reload this record from the database?",
                onConfirm: onReset,
            };
        }

        if (isDirty)
        {
            return {
                title: "Reset Form",
                description: "Are you sure you want to reset this form?",
                onConfirm: onReset,
            };
        }

        return undefined;
    }, [ isEditMode, isDirty, onReset ]);

    // ─── Keyboard Shortcuts (Ctrl+S / Ctrl+R) ─────────────────
    useFormShortcuts({
        onSave: handleFormSubmit,
        onReset,
    });

    return {
        form,
        submitting,
        isDirty,
        isEditMode,
        qString1,
        qString2,
        qString3,
        bannerFields,
        handleFormSubmit,
        onReset,
        resetConfirmProps,
    };
}
