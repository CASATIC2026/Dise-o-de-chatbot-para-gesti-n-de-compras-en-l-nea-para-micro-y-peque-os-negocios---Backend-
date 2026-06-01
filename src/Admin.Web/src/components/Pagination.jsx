/**
 * Pagination.jsx — Componente de paginación reutilizable
 * Props:
 *   currentPage   {number}  - Página actual (1-indexed)
 *   totalPages    {number}  - Total de páginas
 *   totalItems    {number}  - Total de ítems en el listado filtrado
 *   itemsPerPage  {number}  - Ítems por página
 *   onChange      {fn}      - Callback recibe el nuevo número de página
 */
function Pagination({ currentPage, totalPages, totalItems, itemsPerPage, onChange }) {
    if (totalPages <= 1) return null;

    const from = (currentPage - 1) * itemsPerPage + 1;
    const to = Math.min(currentPage * itemsPerPage, totalItems);

    // Genera la secuencia de páginas con elipsis
    const pages = Array.from({ length: totalPages }, (_, i) => i + 1)
        .filter(p => p === 1 || p === totalPages || Math.abs(p - currentPage) <= 1)
        .reduce((acc, p, idx, arr) => {
            if (idx > 0 && p - arr[idx - 1] > 1) acc.push('...');
            acc.push(p);
            return acc;
        }, []);

    return (
        <div className="flex items-center justify-between px-6 py-4 border-t border-neutral-200 dark:border-dark-border">
            <p className="text-sm text-neutral-500 dark:text-neutral-400">
                Mostrando <span className="font-semibold text-neutral-700 dark:text-neutral-200">{from}–{to}</span> de <span className="font-semibold text-neutral-700 dark:text-neutral-200">{totalItems}</span>
            </p>
            <div className="flex items-center gap-1">
                <button
                    onClick={() => onChange(Math.max(1, currentPage - 1))}
                    disabled={currentPage === 1}
                    className="px-3 py-1.5 text-sm rounded-lg border border-neutral-200 dark:border-dark-border text-neutral-600 dark:text-neutral-300 hover:bg-neutral-50 dark:hover:bg-dark-input disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
                >
                    ← Anterior
                </button>

                {pages.map((p, i) =>
                    p === '...' ? (
                        <span key={`e-${i}`} className="px-2 text-neutral-400 dark:text-neutral-600">…</span>
                    ) : (
                        <button
                            key={p}
                            onClick={() => onChange(p)}
                            className={`px-3 py-1.5 text-sm rounded-lg border transition-colors ${
                                currentPage === p
                                    ? 'bg-primary-600 dark:bg-cyan-600 text-white border-primary-600 dark:border-cyan-600'
                                    : 'border-neutral-200 dark:border-dark-border text-neutral-600 dark:text-neutral-300 hover:bg-neutral-50 dark:hover:bg-dark-input'
                            }`}
                        >
                            {p}
                        </button>
                    )
                )}

                <button
                    onClick={() => onChange(Math.min(totalPages, currentPage + 1))}
                    disabled={currentPage === totalPages}
                    className="px-3 py-1.5 text-sm rounded-lg border border-neutral-200 dark:border-dark-border text-neutral-600 dark:text-neutral-300 hover:bg-neutral-50 dark:hover:bg-dark-input disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
                >
                    Siguiente →
                </button>
            </div>
        </div>
    );
}

export default Pagination;
