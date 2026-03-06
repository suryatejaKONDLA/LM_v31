/**
 * Popconfirm configuration for the reset button.
 * When provided, the reset button shows an inline confirmation popover.
 */
export interface ResetConfirmProps
{
    /** Popconfirm title. */
    title: string;
    /** Popconfirm description text. */
    description: string;
    /** Callback when user confirms. */
    onConfirm: () => void;
}
