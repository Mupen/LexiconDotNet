import {
  flexRender,
  getCoreRowModel,
  getSortedRowModel,
  useReactTable
} from '@tanstack/react-table'
import { useMemo, useState } from 'react'
import { Link } from 'react-router-dom'

/*
 * What: ArticleTable renders article results in a dense table view.
 * How: TanStack Table receives column definitions, current rows, pagination
 * state, and sorting state, then produces header/cell models for rendering.
 * Why: table view is useful for scanning many articles by source, author, and
 * published date, while card view is better for casual reading.
 */
export function ArticleTable({ articles, totalResults, page, pageSize, loading, savedIds, saving, onSave, onRemoveSaved, onPageChange }) {
  /*
   * What: sorting stores the current client-side column sort state.
   * How: TanStack Table updates it through onSortingChange.
   * Why: sorting applies only to the loaded page because NewsAPI/server paging is
   * authoritative for which rows are currently available.
   */
  const [sorting, setSorting] = useState([])

  /*
   * What: columns describe how article properties appear in the table.
   * How: each column defines an accessor, header text, and cell renderer.
   * Why: useMemo keeps the column definitions stable across renders so TanStack
   * Table does not rebuild column metadata unnecessarily.
   */
  const columns = useMemo(() => [
    {
      accessorKey: 'sourceName',
      header: 'Source',
      cell: (info) => info.getValue() ?? 'Unknown'
    },
    {
      accessorKey: 'title',
      header: 'Title',
      cell: (info) => info.getValue()
    },
    {
      accessorKey: 'author',
      header: 'Author',
      cell: (info) => info.getValue() ?? 'Unknown'
    },
    {
      accessorKey: 'publishedAt',
      header: 'Published',
      cell: (info) => info.getValue()
        ? new Date(info.getValue()).toLocaleDateString()
        : 'Unknown'
    },
    {
      id: 'actions',
      header: 'Actions',
      cell: ({ row }) => (
        <div className="table-actions">
          <Link to={`/article/${row.original.id}`}>Details</Link>
          <a href={row.original.url} target="_blank" rel="noreferrer">Original</a>
          <button
            type="button"
            className={savedIds.has(row.original.id) ? 'secondary' : ''}
            disabled={saving}
            onClick={() => savedIds.has(row.original.id)
              ? onRemoveSaved(row.original.id)
              : onSave(row.original.id)}
          >
            {savedIds.has(row.original.id) ? 'Saved' : 'Save'}
          </button>
        </div>
      ),
      enableSorting: false
    }
  ], [onRemoveSaved, onSave, savedIds, saving])

  /*
   * What: table is the TanStack Table instance for the current page of articles.
   * How: manualPagination tells the table that the backend/frontend state owns
   * page changes; the table calls onPageChange with the next one-based page.
   * Why: NewsAPI returns paged data, so the table cannot page through all rows
   * locally unless the backend has loaded every result.
   */
  const table = useReactTable({
    data: articles,
    columns,
    manualPagination: true,
    rowCount: totalResults,
    state: {
      pagination: {
        pageIndex: page - 1,
        pageSize
      },
      sorting
    },
    onPaginationChange: (updater) => {
      const current = {
        pageIndex: page - 1,
        pageSize
      }
      const next = typeof updater === 'function' ? updater(current) : updater
      onPageChange(next.pageIndex + 1)
    },
    onSortingChange: setSorting,
    getCoreRowModel: getCoreRowModel(),
    getSortedRowModel: getSortedRowModel()
  })

  return (
    <section className="article-table-panel">
      <p className="table-note">Column sorting applies to the currently loaded page.</p>
      <div className="table-scroll">
        <table>
          <thead>
            {table.getHeaderGroups().map((headerGroup) => (
              <tr key={headerGroup.id}>
                {headerGroup.headers.map((header) => (
                  <th key={header.id}>
                    {header.isPlaceholder ? null : (
                      <button
                        type="button"
                        className="table-sort"
                        disabled={!header.column.getCanSort()}
                        onClick={header.column.getToggleSortingHandler()}
                      >
                        {flexRender(header.column.columnDef.header, header.getContext())}
                        {formatSort(header.column.getIsSorted())}
                      </button>
                    )}
                  </th>
                ))}
              </tr>
            ))}
          </thead>
          <tbody>
            {table.getRowModel().rows.map((row) => (
              <tr key={row.id}>
                {row.getVisibleCells().map((cell) => (
                  <td key={cell.id}>
                    {flexRender(cell.column.columnDef.cell, cell.getContext())}
                  </td>
                ))}
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {articles.length === 0 && !loading && (
        <p className="empty">No articles found for this request.</p>
      )}
    </section>
  )
}

/*
 * What: formatSort converts TanStack Table's sort direction into short text.
 * How: asc/desc become simple suffixes and false/undefined becomes empty text.
 * Why: this keeps the table button label readable while still giving users
 * feedback about the active sort direction.
 */
function formatSort(direction) {
  if (direction === 'asc') {
    return ' asc'
  }

  if (direction === 'desc') {
    return ' desc'
  }

  return ''
}
