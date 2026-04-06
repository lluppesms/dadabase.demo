# PowerPoint Template Specification

PowerPoint template specification for the Architecture deck. This document defines slide masters, typography, color palette, accessibility rules, and layout guidelines used by `Export/generate-architecture-deck.js`.

---

## Slide Dimensions

| Property | Value |
|----------|-------|
| Aspect ratio | 16:9 Widescreen |
| Width | 13.33 inches (33.867 cm) |
| Height | 7.5 inches (19.05 cm) |
| Resolution | Optimized for 1920×1080 display |

---

## Slide Masters (7 Layouts)

### 1. Title Slide

- **Use:** Opening slide, document title and version
- **Background:** Solid `#0078D4` (Primary Blue) or gradient from `#243A5E` to `#0078D4`
- **Title:** Segoe UI Semibold, 36pt, `#FFFFFF`
- **Subtitle:** Segoe UI, 20pt, `#FFFFFF` (80% opacity)
- **Footer area:** Version number and date, Segoe UI, 12pt, `#FFFFFF` (60% opacity)
- **Layout:** Title centered vertically in upper 60%, subtitle below

### 2. Section Divider

- **Use:** Transition between major topic areas (e.g., between Architecture Layers and Data Flows)
- **Background:** Solid `#243A5E` (Dark Blue)
- **Section number:** Segoe UI Semibold, 72pt, `#50E6FF` (Light Blue), left-aligned
- **Section title:** Segoe UI Semibold, 32pt, `#FFFFFF`, below number
- **Accent bar:** 4px horizontal line, `#50E6FF`, spanning 40% width below title
- **Layout:** Content left-aligned with 1.5" left margin

### 3. Content

- **Use:** General content slides with a heading and body text/bullets
- **Background:** `#FFFFFF`
- **Heading:** Segoe UI Semibold, 28pt, `#243A5E` (Dark Blue)
- **Body text:** Segoe UI, 16pt, `#323130` (Dark Gray)
- **Bullet style:** `•` character, `#0078D4` (Primary Blue), 0.25" indent
- **Sub-bullets:** `–` character, `#605E5C`, 0.5" indent, 14pt
- **Content area:** 0.75" margins all sides, heading at top
- **Accent:** 3px left border on heading, `#0078D4`

### 4. Two-Column

- **Use:** Side-by-side comparisons, category listings, before/after
- **Background:** `#FFFFFF`
- **Heading:** Segoe UI Semibold, 28pt, `#243A5E`, full width
- **Column divider:** 1px vertical line, `#D2D0CE`, centered
- **Left column:** 48% width, 0.75" left margin
- **Right column:** 48% width, 0.75" right margin
- **Column headers:** Segoe UI Semibold, 18pt, `#0078D4`
- **Column body:** Segoe UI, 14pt, `#323130`

### 5. Diagram

- **Use:** Architecture diagrams, data flow visualizations, system overviews
- **Background:** `#FFFFFF`
- **Heading:** Segoe UI Semibold, 24pt, `#243A5E`
- **Diagram area:** Centered, max 80% width × 70% height
- **Diagram elements:**
  - Boxes: Rounded corners (8px radius), `#0078D4` fill, `#FFFFFF` text
  - Arrows: 2px, `#243A5E`, with arrowheads
  - Labels: Segoe UI, 12pt, `#323130`
  - Grouping boxes: Dashed border, `#D2D0CE`, light fill `#F3F2F1`
- **Caption:** Segoe UI, 11pt, `#605E5C`, centered below diagram
- **Alt text:** Required for all diagram slides (accessibility)

### 6. Table

- **Use:** Structured data, configuration values, comparison matrices
- **Background:** `#FFFFFF`
- **Heading:** Segoe UI Semibold, 28pt, `#243A5E`
- **Table header row:** `#0078D4` fill, Segoe UI Semibold, 14pt, `#FFFFFF`
- **Table body rows:** Alternating `#FFFFFF` and `#F3F2F1` (Neutral Lighter)
- **Table text:** Segoe UI, 13pt, `#323130`
- **Borders:** 1px, `#D2D0CE` (Neutral Quaternary)
- **Cell padding:** 0.1" vertical, 0.15" horizontal
- **Max rows per slide:** 10 (overflow to continuation slide)

### 7. Closing Slide

- **Use:** Final slide with summary or call-to-action
- **Background:** Gradient from `#243A5E` to `#0A1929`
- **Title:** Segoe UI Semibold, 32pt, `#FFFFFF`
- **Subtitle/CTA:** Segoe UI, 18pt, `#50E6FF`
- **Footer:** Version, date, and System Architecture
- **Layout:** Centered vertically

---

## Typography

| Element | Font | Weight | Size | Color |
|---------|------|--------|------|-------|
| Slide title (Title layout) | Segoe UI | Semibold | 36pt | `#FFFFFF` |
| Slide heading (Content) | Segoe UI | Semibold | 28pt | `#243A5E` |
| Body text | Segoe UI | Regular | 16pt | `#323130` |
| Bullet text | Segoe UI | Regular | 16pt | `#323130` |
| Sub-bullet text | Segoe UI | Regular | 14pt | `#605E5C` |
| Table header | Segoe UI | Semibold | 14pt | `#FFFFFF` |
| Table body | Segoe UI | Regular | 13pt | `#323130` |
| Caption / footnote | Segoe UI | Regular | 11pt | `#605E5C` |
| Code / monospace | Consolas | Regular | 13pt | `#323130` on `#F3F2F1` |
| Footer | Segoe UI | Regular | 10pt | `#A19F9D` |

**Font fallback chain:** Segoe UI → Calibri → Arial → sans-serif

---

## Color Palette

### Primary Colors

| Name | Hex | RGB | Usage |
|------|-----|-----|-------|
| Primary Blue | `#0078D4` | 0, 120, 212 | Primary brand color, headers, accents, buttons |
| Light Blue | `#50E6FF` | 80, 230, 255 | Section numbers, highlights on dark backgrounds |
| Cyan | `#00BCF2` | 0, 188, 242 | Secondary accent, links |

### Dark Colors

| Name | Hex | RGB | Usage |
|------|-----|-----|-------|
| Dark Blue | `#243A5E` | 36, 58, 94 | Headings, section divider backgrounds |
| Darkest Blue | `#0A1929` | 10, 25, 41 | Gradient endpoints, closing slide |
| Near Black | `#2D2D2D` | 45, 45, 45 | High-contrast text alternative |

### Accent Colors

| Name | Hex | RGB | Usage |
|------|-----|-----|-------|
| Yellow | `#FFB900` | 255, 185, 0 | Warnings, attention callouts |
| Red | `#E74856` | 231, 72, 86 | Errors, breaking changes |
| Green | `#00CC6A` | 0, 204, 106 | Success, current status |

### Neutral Colors

| Name | Hex | RGB | Usage |
|------|-----|-----|-------|
| Dark Gray (Text) | `#323130` | 50, 49, 48 | Primary body text |
| Medium Gray | `#605E5C` | 96, 94, 92 | Secondary text, captions |
| Light Gray | `#A19F9D` | 161, 159, 157 | Footer text, disabled |
| Neutral Quaternary | `#D2D0CE` | 210, 208, 206 | Borders, dividers |
| Neutral Lighter | `#F3F2F1` | 243, 242, 241 | Alternating row fills, code backgrounds |
| White | `#FFFFFF` | 255, 255, 255 | Slide backgrounds, text on dark |

---

## Accessibility Rules

### Contrast Ratios

All text must meet **WCAG 2.1 AA** minimum contrast ratios:

| Text Type | Minimum Ratio | Notes |
|-----------|--------------|-------|
| Normal text (≤18pt) | 4.5:1 | Body text, bullets, table content |
| Large text (≥18pt bold or ≥24pt) | 3:1 | Headings, titles |
| UI components | 3:1 | Chart elements, diagram labels |

**Pre-validated combinations:**
- `#323130` on `#FFFFFF` → 12.6:1 ✅
- `#FFFFFF` on `#0078D4` → 4.5:1 ✅
- `#FFFFFF` on `#243A5E` → 10.1:1 ✅
- `#50E6FF` on `#243A5E` → 7.3:1 ✅
- `#323130` on `#F3F2F1` → 10.0:1 ✅

### Font Sizes

| Element | Minimum Size |
|---------|-------------|
| Body text | 14pt |
| Table text | 13pt |
| Footnotes / captions | 11pt |
| Titles | 28pt |
| Code blocks | 13pt |

### Alt Text

- **Required** for all Diagram layout slides
- **Required** for any embedded images or icons
- Alt text should describe the diagram's purpose and key relationships, not just "architecture diagram"
- Example: "Data flow diagram showing peak logging workflow from CSA input through WorkIQ and MSX queries to logbook file creation"

### Reading Order

- Slides must have a logical reading order (top-to-bottom, left-to-right)
- Heading always first in reading order
- Body content follows heading
- Footer elements last

---

## Header and Footer

### Footer (All Slides)

| Element | Position | Content |
|---------|----------|---------|
| Left | Bottom-left, 0.5" from edge | "System Architecture" |
| Center | Bottom-center | Page number: "Slide X of Y" |
| Right | Bottom-right, 0.5" from edge | Version and date: "v3.12.1 · March 2026" |

- Font: Segoe UI, 10pt, `#A19F9D`
- Footer is **not shown** on Title and Closing slides

### Logo Placement

- **No logo required** — omit any brand logo unless explicitly provided
- **Application icon** — optional, placed top-right corner (0.5" × 0.5") on Title slide only
- Icon file: `src/web/Website/wwwroot/favicon.png` (if available)

---

## Slide Margins and Spacing

| Property | Value |
|----------|-------|
| Left margin | 0.75" |
| Right margin | 0.75" |
| Top margin | 0.75" (below heading) |
| Bottom margin | 0.75" (above footer) |
| Heading to body gap | 0.3" |
| Bullet line spacing | 1.2× |
| Paragraph spacing | 0.15" after |
| Table row height | 0.4" minimum |

---

## Implementation Notes

The `Export/generate-architecture-deck.js` script implements this spec using `pptxgenjs`. Brand constants are defined in the script's `C` object:

```javascript
const C = {
  msBlue:    '0078D4',
  darkBlue:  '112F4E',
  darkest:   '0A1929',
  white:     'FFFFFF',
  nearBlack: '323130',
  cyan:      '00BDE3',
  // ... additional colors
};
```

When updating this spec, ensure corresponding constants in the generator script are updated to match. The script reads the architecture markdown file and applies layouts based on section content type.