'use strict';

/**
 * generate-architecture-deck.js
 * Generic architecture PowerPoint generator — reads a markdown architecture document
 * and produces a professionally styled PPTX presentation.
 * Uses pptxgenjs — install with: npm install pptxgenjs
 *
 * Usage:
 *   node .github/skills/architecture-export/generate-architecture-deck.js \
 *     --input  "Docs/MyApp-Architecture.md" \
 *     --output "Docs/MyApp-Architecture.pptx" \
 *     --title  "My Application" \
 *     --repo   "owner/repo-name"
 *
 * --title  Display name used in slide headers, title slide, and footer (default: derived from doc H1)
 * --repo   Repository identifier shown in footer (default: empty)
 */

const PptxGenJS = require('pptxgenjs');
const fs = require('fs');
const path = require('path');

// ---------------------------------------------------------------------------
// Color palette (no # prefix — pptxgenjs convention)
// ---------------------------------------------------------------------------
const C = {
  primaryBlue : '0078D4',
  lightBlue   : '50E6FF',
  darkBlue    : '243A5E',
  darkest     : '0A1929',
  textDark    : '323130',
  textMed     : '605E5C',
  textLight   : 'A19F9D',
  border      : 'D2D0CE',
  neutral     : 'F3F2F1',
  white       : 'FFFFFF',
  green       : '00CC6A',
  yellow      : 'FFB900',
  red         : 'E74856',
};

// Slide dimensions (16:9 widescreen)
const SW = 13.33;  // inches
const SH = 7.5;

// Margins
const ML = 0.75;
const MT = 0.75;
const MR = 0.75;
const MB = 0.65;

// ---------------------------------------------------------------------------
// Runtime parameters (set in main() from CLI args)
// ---------------------------------------------------------------------------
let APP_TITLE = 'Architecture';
let REPO_NAME = '';

// ---------------------------------------------------------------------------
// Markdown parser
// ---------------------------------------------------------------------------
function parseDocument(mdPath) {
  const text  = fs.readFileSync(mdPath, 'utf-8');
  const lines = text.split(/\r?\n/);

  const versionLine = lines.find(l => l.startsWith('> **Version:**')) || '';
  const version = (versionLine.match(/Version:\*\* ([^\s·]+)/) || [])[1] || '1.0';
  const date    = (versionLine.match(/Generated: ([^·\n]+)/) || [])[1]?.trim() || '2026-04-06';

  const sections = [];
  let current = null;
  let contentLines = [];

  for (const line of lines) {
    const m = line.match(/^## (\d+)\. (.+)/);
    if (m) {
      if (current) sections.push({ ...current, content: contentLines.join('\n') });
      current = { num: parseInt(m[1]), title: m[2].trim() };
      contentLines = [];
    } else if (current) {
      contentLines.push(line);
    }
  }
  if (current) sections.push({ ...current, content: contentLines.join('\n') });

  const h1Line  = lines.find(l => /^# [^#]/.test(l)) || '';
  const docTitle = h1Line.replace(/^# /, '').replace(/\s*[:,\-\u2013]\s*(Architecture|Architecture Overview).*$/i, '').trim();

  return { version, date, docTitle, sections };
}

function extractBullets(content) {
  return content.split('\n')
    .filter(l => /^\s*[-*] /.test(l))
    .map(l => l.replace(/^\s*[-*] /, '').replace(/\*\*/g, '').replace(/`/g, '').replace(/\[([^\]]+)\]\([^)]+\)/g, '$1').trim())
    .filter(Boolean)
    .slice(0, 9);
}

function extractTables(content) {
  const tables = [];
  const lines  = content.split('\n');
  let headers  = null;
  let rows     = [];
  let started  = false;

  for (const line of lines) {
    const s = line.trim();
    if (/^\|.+\|$/.test(s)) {
      const cells = s.split('|').slice(1, -1).map(c => c.trim());
      if (!started) {
        headers = cells;
        started = true;
      } else if (cells.every(c => /^[-: ]+$/.test(c))) {
        // separator row — skip
      } else {
        rows.push(cells);
      }
    } else if (started) {
      if (headers && rows.length > 0) tables.push({ headers, rows: rows.slice() });
      started = false; headers = null; rows = [];
    }
  }
  if (started && headers && rows.length > 0) tables.push({ headers, rows });
  return tables;
}

function extractCodeBlocks(content) {
  const blocks = [];
  const re = /```(?:\w+)?\r?\n([\s\S]+?)```/g;
  let m;
  while ((m = re.exec(content)) !== null) blocks.push(m[1].trimEnd());
  return blocks;
}

function extractFirstParagraph(content) {
  for (const line of content.split('\n')) {
    const s = line.trim();
    if (!s) continue;
    if (/^[|#`*>-]/.test(s)) break;
    return s.replace(/\*\*/g, '').replace(/`/g, '');
  }
  return '';
}

// ---------------------------------------------------------------------------
// Slide helpers
// ---------------------------------------------------------------------------
function addFooter(slide) {
  slide.addText('System Architecture', {
    x: ML, y: SH - 0.4, w: 3.5, h: 0.3,
    fontSize: 9, fontFace: 'Segoe UI', color: C.textLight,
  });
  slide.addText(REPO_NAME ? APP_TITLE + '  ·  ' + REPO_NAME : APP_TITLE, {
    x: SW - MR - 4.5, y: SH - 0.4, w: 4.5, h: 0.3,
    fontSize: 9, fontFace: 'Segoe UI', color: C.textLight, align: 'right',
  });
  // Footer divider line
  slide.addShape('rect', {
    x: ML, y: SH - 0.45, w: SW - ML - MR, h: 0.01,
    fill: { color: C.border }, line: { color: C.border },
  });
}

function addHeading(slide, text) {
  // Left accent bar
  slide.addShape('rect', {
    x: ML - 0.18, y: MT - 0.05, w: 0.06, h: 0.65,
    fill: { color: C.primaryBlue }, line: { color: C.primaryBlue },
  });
  slide.addText(text, {
    x: ML, y: MT, w: SW - ML - MR, h: 0.65,
    fontSize: 28, fontFace: 'Segoe UI', bold: true, color: C.darkBlue,
  });
}

// ---------------------------------------------------------------------------
// Slide creators
// ---------------------------------------------------------------------------
function addTitleSlide(prs, version, date) {
  const slide = prs.addSlide();
  slide.background = { fill: C.darkBlue };

  // Blue upper band
  slide.addShape('rect', {
    x: 0, y: 0, w: SW, h: SH * 0.62,
    fill: { color: C.primaryBlue }, line: { color: C.primaryBlue },
  });

  // Accent stripe at top
  slide.addShape('rect', {
    x: 0, y: 0, w: SW, h: 0.1,
    fill: { color: C.lightBlue }, line: { color: C.lightBlue },
  });

  slide.addText(APP_TITLE, {
    x: ML, y: 1.5, w: SW - ML * 2, h: 0.9,
    fontSize: 42, fontFace: 'Segoe UI', bold: true,
    color: C.white, align: 'center',
  });

  slide.addText('Architecture Overview', {
    x: ML, y: 2.5, w: SW - ML * 2, h: 0.7,
    fontSize: 24, fontFace: 'Segoe UI', color: C.white, align: 'center',
  });

  slide.addText(`Version ${version}  ·  ${date}`, {
    x: ML, y: 3.35, w: SW - ML * 2, h: 0.4,
    fontSize: 13, fontFace: 'Segoe UI', color: C.white, align: 'center',
  });

  if (REPO_NAME) {
    slide.addText(REPO_NAME, {
      x: ML, y: SH - 1.1, w: SW - ML * 2, h: 0.35,
      fontSize: 11, fontFace: 'Segoe UI', color: C.textLight, align: 'center',
    });
  }
}

function addSectionDivider(prs, num, title) {
  const slide = prs.addSlide();
  slide.background = { fill: C.darkBlue };

  slide.addText(String(num).padStart(2, '0'), {
    x: ML, y: 1.4, w: 3.5, h: 2.1,
    fontSize: 80, fontFace: 'Segoe UI', bold: true, color: C.lightBlue,
  });

  slide.addText(title, {
    x: ML, y: 3.65, w: SW - ML * 2, h: 0.9,
    fontSize: 32, fontFace: 'Segoe UI', bold: true, color: C.white,
  });

  slide.addShape('rect', {
    x: ML, y: 4.7, w: SW * 0.38, h: 0.06,
    fill: { color: C.lightBlue }, line: { color: C.lightBlue },
  });
}

function addContentSlide(prs, heading, bullets) {
  const slide = prs.addSlide();
  slide.background = { fill: C.white };
  addHeading(slide, heading);

  if (bullets.length > 0) {
    const items = bullets.map(b => ({
      text: b,
      options: {
        bullet: { type: 'bullet', indent: 25 },
        fontSize: 15,
        fontFace: 'Segoe UI',
        color: C.textDark,
        paraSpaceAfter: 6,
        paraSpaceBefore: 0,
      },
    }));
    slide.addText(items, {
      x: ML, y: MT + 0.78, w: SW - ML - MR, h: SH - MT - 0.78 - MB,
      valign: 'top',
    });
  }

  addFooter(slide);
}

function addDiagramSlide(prs, heading, diagramText, caption) {
  const slide = prs.addSlide();
  slide.background = { fill: C.white };
  addHeading(slide, heading);

  const boxY = MT + 0.78;
  const boxH = SH - boxY - MB - 0.25;

  slide.addShape('rect', {
    x: ML, y: boxY, w: SW - ML - MR, h: boxH,
    fill: { color: C.neutral }, line: { color: C.border, pt: 1 },
  });

  const lines = (diagramText || '').split('\n').slice(0, 50);
  slide.addText(lines.join('\n'), {
    x: ML + 0.1, y: boxY + 0.1, w: SW - ML - MR - 0.2, h: boxH - 0.2,
    fontSize: 7.5, fontFace: 'Consolas', color: C.textDark, valign: 'top', wrap: false,
    lineSpacingMultiple: 1.0,
  });

  if (caption) {
    slide.addText(caption, {
      x: ML, y: SH - MB + 0.05, w: SW - ML - MR, h: 0.3,
      fontSize: 10, fontFace: 'Segoe UI', italic: true, color: C.textMed, align: 'center',
    });
  }

  addFooter(slide);
}

function addTableSlide(prs, heading, tableTitle, headers, rows) {
  const slide = prs.addSlide();
  slide.background = { fill: C.white };
  addHeading(slide, heading);

  const tableY = tableTitle ? MT + 1.15 : MT + 0.78;

  if (tableTitle) {
    slide.addText(tableTitle, {
      x: ML, y: MT + 0.72, w: SW - ML - MR, h: 0.35,
      fontSize: 13, fontFace: 'Segoe UI', color: C.textMed, italic: true,
    });
  }

  const colCount = headers.length;
  const colW = (SW - ML - MR) / colCount;
  const maxRows = Math.min(rows.length, 10);

  const tableRows = [];

  // Header row
  tableRows.push(headers.map(h => ({
    text: h,
    options: {
      bold: true, fontSize: 12, fontFace: 'Segoe UI',
      color: C.white, fill: C.primaryBlue, align: 'left', valign: 'middle',
    },
  })));

  // Data rows
  rows.slice(0, maxRows).forEach((row, i) => {
    const bg = i % 2 === 0 ? C.white : C.neutral;
    tableRows.push(
      row.map(cell => ({
        text: (cell || '').replace(/\*\*/g, '').replace(/`/g, ''),
        options: {
          fontSize: 11, fontFace: 'Segoe UI',
          color: C.textDark, fill: bg, align: 'left', valign: 'top',
        },
      }))
    );
  });

  slide.addTable(tableRows, {
    x: ML,
    y: tableY,
    w: SW - ML - MR,
    colW: Array(colCount).fill(colW),
    rowH: 0.38,
    border: { type: 'solid', pt: 0.5, color: C.border },
  });

  addFooter(slide);
}

function addTwoColumnSlide(prs, heading, leftHeader, leftItems, rightHeader, rightItems) {
  const slide = prs.addSlide();
  slide.background = { fill: C.white };
  addHeading(slide, heading);

  const colW = (SW - ML - MR) / 2 - 0.15;
  const rightX = ML + colW + 0.3;

  // Divider
  slide.addShape('rect', {
    x: SW / 2, y: MT + 0.75, w: 0.01, h: SH - MT - 0.75 - MB,
    fill: { color: C.border }, line: { color: C.border },
  });

  // Left column header
  slide.addText(leftHeader, {
    x: ML, y: MT + 0.82, w: colW, h: 0.4,
    fontSize: 16, fontFace: 'Segoe UI', bold: true, color: C.primaryBlue,
  });
  if (leftItems.length > 0) {
    const items = leftItems.map(b => ({
      text: b,
      options: {
        bullet: { type: 'bullet', indent: 20 },
        fontSize: 13,
        fontFace: 'Segoe UI',
        color: C.textDark,
        paraSpaceAfter: 5,
        paraSpaceBefore: 0,
      },
    }));
    slide.addText(items, {
      x: ML, y: MT + 1.3, w: colW, h: SH - MT - 1.3 - MB, valign: 'top',
    });
  }

  // Right column header
  slide.addText(rightHeader, {
    x: rightX, y: MT + 0.82, w: colW, h: 0.4,
    fontSize: 16, fontFace: 'Segoe UI', bold: true, color: C.primaryBlue,
  });
  if (rightItems.length > 0) {
    const items = rightItems.map(b => ({
      text: b,
      options: {
        bullet: { type: 'bullet', indent: 20 },
        fontSize: 13,
        fontFace: 'Segoe UI',
        color: C.textDark,
        paraSpaceAfter: 5,
        paraSpaceBefore: 0,
      },
    }));
    slide.addText(items, {
      x: rightX, y: MT + 1.3, w: colW, h: SH - MT - 1.3 - MB, valign: 'top',
    });
  }

  addFooter(slide);
}

function addClosingSlide(prs, version, date) {
  const slide = prs.addSlide();
  slide.background = { fill: C.darkest };

  slide.addShape('rect', {
    x: 0, y: 0, w: SW, h: 0.1,
    fill: { color: C.primaryBlue }, line: { color: C.primaryBlue },
  });

  slide.addText(APP_TITLE, {
    x: ML, y: 2.4, w: SW - ML * 2, h: 0.9,
    fontSize: 36, fontFace: 'Segoe UI', bold: true, color: C.white, align: 'center',
  });

  slide.addText('Architecture Documentation', {
    x: ML, y: 3.4, w: SW - ML * 2, h: 0.6,
    fontSize: 20, fontFace: 'Segoe UI', color: C.lightBlue, align: 'center',
  });

  const closingFooter = REPO_NAME ? `${version}  ·  ${date}  ·  ${REPO_NAME}` : `${version}  ·  ${date}`;
  slide.addText(closingFooter, {
    x: ML, y: SH - 1.0, w: SW - ML * 2, h: 0.35,
    fontSize: 11, fontFace: 'Segoe UI', color: C.textLight, align: 'center',
  });
}

// ---------------------------------------------------------------------------
// Section → slides mapping  (follows Canonical Section List in SKILL.md)
// ---------------------------------------------------------------------------
function addSectionSlides(prs, section) {
  const { num, title, content } = section;
  const bullets     = extractBullets(content);
  const tables      = extractTables(content);
  const codeBlocks  = extractCodeBlocks(content);
  const para        = extractFirstParagraph(content);

  addSectionDivider(prs, num, title);

  switch (num) {
    case 1: { // Executive Summary — Title + Content (2 slides, but divider counts as #1)
      const summary = [para, ...bullets].filter(Boolean).slice(0, 8);
      addContentSlide(prs, '1. Executive Summary', summary);
      break;
    }
    case 2: { // Repository Structure — Diagram
      const code = codeBlocks[0] || '';
      addDiagramSlide(prs, '2. Repository Structure', code,
        'Monorepo: src/ · infra/ · .github/ · playwright/ · Docs/');
      break;
    }
    case 3: { // Application Architecture — Diagram + Content
      addDiagramSlide(prs, `3. ${title}`, codeBlocks[0] || '', para || '');
      if (bullets.length > 0) {
        addContentSlide(prs, `3. ${title}`, bullets);
      }
      break;
    }
    case 4: { // Source Code Projects
      if (tables.length > 0) {
        for (const tbl of tables.slice(0, 3)) {
          addTableSlide(prs, `4. ${title}`, null, tbl.headers, tbl.rows);
        }
      } else {
        addContentSlide(prs, `4. ${title}`, bullets);
      }
      break;
    }
    case 5: { // AI/GenAI — Diagram + Table
      addDiagramSlide(prs, '5. AI / GenAI Integration', codeBlocks[0] || '',
        'Microsoft Agent Framework + Azure OpenAI (GPT-4o & DALL-E 3) + Azure Blob Storage');
      if (tables.length > 0) {
        addTableSlide(prs, '5. AI / GenAI Integration', 'AI Capabilities',
          tables[0].headers, tables[0].rows);
      }
      break;
    }
    case 6: { // Infrastructure — Diagram + Table
      const infraText = codeBlocks[0] || [
        'infra/Bicep/',
        '├── main.bicep               Entry point — orchestrates all modules',
        '├── resourcenames.bicep       Centralised resource naming convention',
        '├── modules/',
        '│   ├── webapp/               Azure App Service',
        '│   ├── container/            Azure Container Apps + Container Registry',
        '│   ├── function/             Azure Functions (Flex Consumption)',
        '│   ├── database/             Azure SQL Server + database',
        '│   ├── monitor/              Application Insights + Log Analytics',
        '│   ├── security/             Azure Key Vault',
        '│   ├── storage/              Azure Storage Accounts',
        '│   └── iam/                  Managed Identity + RBAC',
        '└── data/',
        '    ├── resourceAbbreviations.json',
        '    └── roleDefinitions.json',
      ].join('\n');
      addDiagramSlide(prs, '6. Infrastructure as Code (Bicep)', infraText,
        'deploymentType parameter: webapp | containerapp | functionapp | all');
      const keyRes = tables.find(t => t.headers[0] === 'Resource') || tables[1] || tables[0];
      if (keyRes) {
        addTableSlide(prs, '6. Infrastructure as Code', 'Key Azure Resources',
          keyRes.headers, keyRes.rows);
      }
      break;
    }
    case 7: { // CI/CD — Table ×2
      if (tables.length > 0) {
        addTableSlide(prs, '7. CI/CD Pipelines — GitHub Actions', null,
          tables[0].headers, tables[0].rows);
      }
      if (tables.length > 1) {
        addTableSlide(prs, '7. CI/CD Pipelines — Azure DevOps', null,
          tables[1].headers, tables[1].rows);
      }
      break;
    }
    case 8: { // Testing — Two-Column or Content
      if (tables.length > 0) {
        addTableSlide(prs, `8. ${title}`, null, tables[0].headers, tables[0].rows);
      } else {
        addContentSlide(prs, `8. ${title}`, bullets);
      }
      break;
    }
    case 9: { // Auth & Security — Diagram + Content
      addDiagramSlide(prs, '9. Authentication & Security', codeBlocks[0] || '',
        'Three auth paths: Browser (OIDC), API consumer (API Key), Azure services (Managed Identity)');
      break;
    }
    case 10: { // Theme and UI — Content
      addContentSlide(prs, `10. ${title}`, bullets);
      break;
    }
    case 11: { // Configuration Reference — Table
      if (tables.length > 0) {
        addTableSlide(prs, '11. Configuration Reference', null,
          tables[0].headers, tables[0].rows);
      }
      break;
    }
    case 12: { // Key Design Patterns — Table
      if (tables.length > 0) {
        addTableSlide(prs, '12. Key Design Patterns', null,
          tables[0].headers, tables[0].rows);
      }
      break;
    }
    case 13: { // Copilot Config — Content + Table
      if (tables.length > 0) {
        addTableSlide(prs, '13. Copilot / GitHub Copilot Configuration',
          'GitHub Copilot configuration hierarchy', tables[0].headers, tables[0].rows);
      } else {
        addContentSlide(prs, '13. Copilot / GitHub Copilot Configuration', bullets);
      }
      break;
    }
    case 14: { // AZD — Table
      if (tables.length > 0) {
        addTableSlide(prs, '14. Azure Developer CLI (AZD) Integration',
          'AZD integration files', tables[0].headers, tables[0].rows);
      }
      break;
    }
    case 15: { // Documentation — Table
      if (tables.length > 0) {
        addTableSlide(prs, '15. Documentation', 'Project documentation index',
          tables[0].headers, tables[0].rows);
      }
      break;
    }
    default: {
      addContentSlide(prs, `${num}. ${title}`, bullets);
    }
  }
}

// ---------------------------------------------------------------------------
// Main
// ---------------------------------------------------------------------------
async function main() {
  const args      = process.argv.slice(2);
  const inputIdx  = args.indexOf('--input');
  const outputIdx = args.indexOf('--output');
  const titleIdx  = args.indexOf('--title');
  const repoIdx   = args.indexOf('--repo');

  const inputPath  = inputIdx  >= 0 ? args[inputIdx  + 1] : 'Docs/DadABase-Architecture.md';
  const outputPath = outputIdx >= 0 ? args[outputIdx + 1] : 'Docs/DadABase-Architecture.pptx';

  if (!fs.existsSync(inputPath)) {
    console.error(`Error: Input file not found: ${inputPath}`);
    process.exit(1);
  }

  console.log(`Reading:  ${inputPath}`);
  const doc = parseDocument(inputPath);
  console.log(`Parsed ${doc.sections.length} sections — v${doc.version} (${doc.date})`);

  // Resolve display name and repo identifier from args, doc H1, or input filename
  APP_TITLE = titleIdx >= 0 ? args[titleIdx + 1]
    : (doc.docTitle || path.basename(inputPath, path.extname(inputPath)).replace(/-?Architecture$/i, '').replace(/[-_]/g, ' ').trim() || 'Architecture');
  REPO_NAME = repoIdx >= 0 ? args[repoIdx + 1] : '';

  const prs = new PptxGenJS();
  prs.layout = 'LAYOUT_WIDE';

  // Title
  addTitleSlide(prs, doc.version, doc.date);

  // All 15 sections
  for (const section of doc.sections) {
    addSectionSlides(prs, section);
  }

  // Closing
  addClosingSlide(prs, doc.version, doc.date);

  // Write
  await prs.writeFile({ fileName: outputPath });
  const stats  = fs.statSync(outputPath);
  const sizeKB = Math.round(stats.size / 1024);

  console.log('\n\u2705 PPTX generated successfully');
  console.log(`   Output:   ${outputPath}`);
  console.log(`   Size:     ${sizeKB} KB`);
  console.log(`   Sections: ${doc.sections.length}/15`);
  // Count slides: title + (divider + content per section) + closing
  const slideEst = 1 + doc.sections.length * 1.5 + 1;
  console.log(`   Slides:   ~${Math.round(slideEst)} (approx)`);
}

main().catch(err => {
  console.error('PPTX generation failed:', err.message);
  process.exit(1);
});
