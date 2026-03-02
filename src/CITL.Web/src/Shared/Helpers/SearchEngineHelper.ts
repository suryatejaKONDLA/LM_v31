/**
 * Generic in-memory search engine that filters objects by multiple text properties.
 *
 * Features:
 *  - AND semantics: space-separated tokens must ALL match.
 *  - OR semantics: pipe-separated alternatives within a single token.
 *  - Progressive narrowing: when the new query extends the previous one,
 *    filtering runs against the last result set instead of the full dataset.
 *  - Zero external dependencies.
 *
 * @example
 * ```ts
 * const engine = new SearchEngineHelper(menus, m => m.name, m => m.description);
 * const results = engine.search("user settings");
 * ```
 */
export class SearchEngineHelper<T>
{
    private readonly _data: readonly T[];
    private readonly _selectors: readonly ((item: T) => string | null | undefined)[];
    private _history: T[][] = [];
    private _lastQuery = "";

    constructor(
        data: readonly T[],
        ...selectors: readonly ((item: T) => string | null | undefined)[]
    )
    {
        this._data = data;
        this._selectors = selectors;
    }

    /**
     * Search the dataset.
     *
     * @param query Space = AND, pipe (|) = OR within a term.
     * @returns Filtered items matching every AND-term.
     */
    public search(query: string): T[]
    {
        const trimmed = query.trim();

        if (trimmed.length === 0)
        {
            this._history = [];
            this._lastQuery = "";
            return [ ...this._data ];
        }

        // If the query was shortened (backspace) or diverged, rebuild from full data.
        const canNarrow =
            trimmed.length >= this._lastQuery.length &&
            trimmed.toLowerCase().startsWith(this._lastQuery.toLowerCase());

        if (!canNarrow)
        {
            this._history = [];
        }

        const lastHistory = this._history[this._history.length - 1];
        const base = this._history.length > 0 && lastHistory ? lastHistory : this._data;
        const filtered = this.filter(trimmed, base);

        this._history.push(filtered);
        this._lastQuery = trimmed;

        return filtered;
    }

    /** Reset internal state so the next search starts from the full dataset. */
    public reset(): void
    {
        this._history = [];
        this._lastQuery = "";
    }

    // ----- private -----

    private filter(query: string, data: readonly T[]): T[]
    {
        const terms = query.split(/\s+/).filter((t) => t.length > 0);

        if (terms.length === 0)
        {
            return [ ...data ];
        }

        const result: T[] = [];

        for (const item of data)
        {
            if (this.matchesAllTerms(item, terms))
            {
                result.push(item);
            }
        }

        return result;
    }

    private matchesAllTerms(item: T, terms: string[]): boolean
    {
        for (const term of terms)
        {
            const orParts = term.split("|").filter((t) => t.length > 0);

            if (orParts.length === 0)
            {
                continue;
            }

            let anyOrMatch = false;

            for (const orTerm of orParts)
            {
                const lower = orTerm.toLowerCase();

                for (const selector of this._selectors)
                {
                    const value = selector(item);

                    if (value?.toLowerCase().includes(lower))
                    {
                        anyOrMatch = true;
                        break;
                    }
                }

                if (anyOrMatch)
                {
                    break;
                }
            }

            if (!anyOrMatch)
            {
                return false;
            }
        }

        return true;
    }
}
