# PDF Export Specification

Guidelines, conventions, and best practices for generating PDF exports from data (executive summaries, customer briefs, weekly reports). These rules were established through iterative production use and should be followed by any PDF generation workflow.

---

## Technology Stack

| Component | Tool | Notes |
|-----------|------|-------|
| **PDF engine** | `pdfkit` (Node.js) | Preferred for programmatic generation with tables, headers, and controlled layout |
| **Alternative** | Markdown + `pdf_options` frontmatter | For simpler text-heavy exports (e.g., `md-to-pdf`). See `Export/2026-03-24-exec-summary-1on1.md` for frontmatter example |
| **Output directory** | `Export/` (repo root) | All PDF exports go here |
| **Generator scripts** | Temporary — create in repo root, delete after generation | Do not commit generator scripts |

---

## Page Layout

| Property | Value |
|----------|-------|
| Page size | US Letter (8.5" × 11") |
| Margins | 54pt (0.75") all sides |
| Usable width | ~500pt |
| Page bottom boundary | `pageHeight - bottomMargin` (702pt for Letter) |
| Orientation | Portrait |

---

## Typography

| Element | Font | Size | Color |
|---------|------|------|-------|
| Title (page header bar) | Helvetica-Bold | 22pt | `#FFFFFF` on `#0078D4` bg |
| Subtitle | Helvetica | 12pt | `#FFFFFF` |
| Section heading | Helvetica-Bold | 14pt | `#0078D4` (Primary Blue) |
| Subheading | Helvetica-Bold | 11pt | `#333333` |
| Body text | Helvetica | 10pt | `#333333` |
| Table header | Helvetica-Bold | 8pt | `#333333` on `#F0F0F0` bg |
| Table cell | Helvetica | 8pt | `#333333` |
| Footer | Helvetica | 8pt | `#666666` |

---

## Color Palette

| Name | Hex | Use |
|------|-----|-----|
| Primary Blue | `#0078D4` | Title bar, section headings, accent |
| Dark Gray | `#333333` | Body text, table text |
| Medium Gray | `#666666` | Metric labels, footer |
| Light Gray | `#E0E0E0` | Section divider lines |
| Table Header BG | `#F0F0F0` | Table header row background |
| Table Stripe | `#FAFAFA` | Alternating row background |
| Accent Green | `#107C10` | Positive signals (optional) |
| Accent Orange | `#D83B01` | Warning signals (optional) |
| Accent Red | `#C50F1F` | Critical items (optional) |

---

## Table Rendering Rules

### CRITICAL: Page Overflow Handling

Tables that span near a page boundary will be cut off if not handled correctly. **Always implement these safeguards:**

1. **Pre-flight space check** — Before rendering any table, calculate whether the table header + at minimum 3 data rows will fit in the remaining page space. If not, insert a page break BEFORE the table.

```javascript
function ensureSpace(needed) {
  if (doc.y + needed > pageBottom) {
    doc.addPage();
  }
}

// Before every table:
const minNeeded = rowHeight * Math.min(rows.length + 1, 4);
ensureSpace(minNeeded);
```

2. **Row-level overflow detection** — During row rendering, check if each row fits before drawing it. If it doesn't, break to a new page.

3. **Re-draw headers on continuation pages** — When a table breaks across pages, re-draw the header row at the top of the new page so readers can identify columns.

```javascript
if (y + rowHeight > pageBottom) {
  doc.addPage();
  y = topMargin;
  // Re-draw header row on new page
  drawHeaderRow(headers, colWidths, y);
  y += rowHeight;
}
```

4. **Section heading + table coupling** — Never render a section heading at the bottom of a page with the table starting on the next page. Check that the heading + table header + at least 2 rows fit together.

### Table Sizing

| Property | Value |
|----------|-------|
| Row height | 18pt |
| Cell padding | 4pt |
| Max column widths total | ~500pt (usable page width) |
| Column width strategy | Fixed widths per table — adjust to content |

### Table Styling

- Header row: `#F0F0F0` background, bold text
- Alternating rows: even rows get `#FAFAFA` background
- No visible cell borders — clean, modern look
- 6pt gap below table before next content

---

## Page Structure Conventions

### Executive Summary (Weekly)

| Page | Content |
|------|---------|
| 1 | Title bar + Week at a Glance metrics + Key Accomplishments (text) |
| 2 | Pipeline Movement table + HoK Summary table |
| 3 | Customer Engagement Coverage table + Weekly Themes (text) |
| 4 | Risks & Blockers table + Next Week Outlook table |

### Customer Brief (Ad-hoc)

| Page | Content |
|------|---------|
| 1 | Title bar + Engagement overview + Scope |
| 2 | Sessions delivered + Recommendations |
| 3+ | Technical details as needed |

### General Principles

1. **One large table per page section** — Don't stack two large tables in the same page section. Give each major table its own breathing room.
2. **Text before tables** — Place narrative/text content before table-heavy pages. Text reflows naturally; tables don't.
3. **Heading + first content together** — A section heading must always have at least some content below it on the same page. Never leave a heading orphaned at the page bottom.
4. **Footer on last page only** — The `Generated by AI` attribution goes at the bottom of the final page.

---

## File Naming Convention

```
Export/{Type}_{Date-Range}.pdf
```

Examples:
- `Weekly-Executive-Summary_2026-03-23-to-27.pdf`
- `Customer-Brief_Internova-AVD_2026-03-27.pdf`
- `Monthly-SE-Report_2026-03.pdf`

---

## Generator Script Pattern

PDF generator scripts are **ephemeral** — created at repo root, executed, and deleted after successful generation. They should NOT be committed.

```
1. Create:   generate-{name}.js  (repo root)
2. Execute:  node generate-{name}.js
3. Verify:   Check file size and open for visual inspection
4. Clean up: Remove generate-{name}.js
```

---

## Accessibility

- Minimum font size: 8pt (tables only; body text 10pt+)
- Contrast ratio: All text meets WCAG AA against its background
- PDF metadata: Set `Title`, `Author`, and `Subject` in document info
- Tables: Use header rows with distinct styling for screen reader context

---

## Known Issues & Workarounds

| Issue | Workaround |
|-------|-----------|
| pdfkit doesn't support automatic table row height for wrapped text | Use fixed row heights (18pt) and keep cell content concise (8pt font) |
| No native markdown-to-PDF table rendering | Build tables manually with rect + text positioning |
| Font embedding limited to built-in PDF fonts | Use Helvetica family (built-in); Segoe UI not available in pdfkit |
| Emoji rendering not supported in pdfkit | Use text indicators (HIGH, MEDIUM, etc.) instead of emoji in PDFs |