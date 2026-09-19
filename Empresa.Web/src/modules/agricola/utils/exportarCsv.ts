export function exportarCsv(colunas: string[], linhas: Record<string, unknown>[], filename: string) {
  const BOM = '\uFEFF';
  const separator = ';';

  const headers = colunas.map((c) => `"${c}"`).join(separator);
  const rows = linhas.map((row) =>
    colunas.map((col) => {
      const val = row[col];
      const str = val != null ? String(val) : '';
      return `"${str.replace(/"/g, '""')}"`;
    }).join(separator)
  );

  const csv = [headers, ...rows].join('\r\n');
  const blob = new Blob([BOM + csv], { type: 'text/csv;charset=utf-8;' });
  const url = URL.createObjectURL(blob);
  const link = document.createElement('a');
  link.href = url;
  link.download = filename;
  link.click();
  URL.revokeObjectURL(url);
}
