'use strict';

/**
 * generate-architecture-pdf.js
 * Generic architecture PDF generator — reads a markdown architecture document
 * and produces a professionally styled PDF document.
 * Uses pdfkit — install with: npm install pdfkit
 *
 * Usage:
 *   node .github/skills/architecture-export/generate-architecture-pdf.js \
 *     --input  "Docs/MyApp-Architecture.md" \
 *     --output "Docs/MyApp-Architecture.pdf" \
 *     --title  "My Application" \
 *     --repo   "owner/repo-name"
 *
 * --title  Display name used in headers and footer (default: derived from doc H1)
 * --repo   Repository identifier shown in footer (default: empty)
 */

const PDFDocument = require('pdfkit');
const fs          = require('fs');
const path        = require('path');

// ---------------------------------------------------------------------------
// Runtime parameters (set in main() from CLI args)
// ---------------------------------------------------------------------------
let APP_TITLE = 'Architecture';
let REPO_NAME = '';

// ---------------------------------------------------------------------------
// Monospace font — try to register a system TTF with Unicode support so that
// box-drawing characters (├ │ └ ─ ┌ ┐ ▼ etc.) render correctly in code blocks.
// Falls back to built-in 'Courier' + ASCII substitution when no TTF is found.
// ---------------------------------------------------------------------------
const MONO_FONT_CANDIDATES = [
  'C:\\Windows\\Fonts\\consola.ttf',       // Consolas (Windows)
  'C:\\Windows\\Fonts\\cour.ttf',          // Courier New (Windows)
  'C:\\Windows\\Fonts\\lucon.ttf',         // Lucida Console (Windows)
  '/usr/share/fonts/truetype/dejavu/DejaVuSansMono.ttf',  // Linux
  '/System/Library/Fonts/Menlo.ttc',       // macOS
];
let MONO_FONT_NAME       = 'Courier';  // updated in registerMonoFont()
let MONO_UNICODE_CAPABLE = false;

function registerMonoFont(pdfDoc) {
  for (const fp of MONO_FONT_CANDIDATES) {
    if (fs.existsSync(fp)) {
      try {
        pdfDoc.registerFont('_UnicodeMono', fp);
        MONO_FONT_NAME       = '_UnicodeMono';
        MONO_UNICODE_CAPABLE = true;
        return;
      } catch (_) { /* try next */ }
    }
  }
}

// ASCII fallback — map Unicode box/line/arrow chars to printable equivalents
function sanitizeMono(line) {
  return line
    .replace(/[├┣┝]/g, '+').replace(/[└┗┕]/g, '+')
    .replace(/[┌┏┍]/g, '+').replace(/[┐┓┑]/g, '+')
    .replace(/[┘┛┙]/g, '+').replace(/[┤┫┥]/g, '+')
    .replace(/[┬┳┯]/g, '+').replace(/[┴┻┷]/g, '+')
    .replace(/[┼╋┿]/g, '+')
    .replace(/[─━╌╍]/g, '-').replace(/[│┃╎╏]/g, '|')
    .replace(/[▼▽]/g, 'v').replace(/[▲△]/g, '^')
    .replace(/[→⟶]/g, '->').replace(/[←⟵]/g, '<-')
    .replace(/[·•·]/g, '*').replace(/[—–]/g, '--')
    .replace(/[^\x00-\x7E]/g, '?');
}

// ---------------------------------------------------------------------------
// Layout constants (points)
// ---------------------------------------------------------------------------
const PAGE_W   = 612;   // 8.5 in × 72 pt/in
const PAGE_H   = 792;   // 11 in × 72 pt/in
const MARGIN   = 54;    // 0.75 in
const USABLE_W = PAGE_W - MARGIN * 2;
const BOTTOM   = PAGE_H - MARGIN;   // 738 pt
const HEADER_H = 72;                // title bar height

// ---------------------------------------------------------------------------
// Color palette
// ---------------------------------------------------------------------------
const C = {
  primaryBlue : '#0078D4',
  darkGray    : '#333333',
  medGray     : '#666666',
  lightGray   : '#E0E0E0',
  tblHeader   : '#F0F0F0',
  tblStripe   : '#FAFAFA',
  white       : '#FFFFFF',
  black       : '#000000',
};

// ---------------------------------------------------------------------------
// State
// ---------------------------------------------------------------------------
let doc;
let y;

function ensureSpace(needed) {
  if (y + needed > BOTTOM) {
    doc.addPage();
    y = MARGIN + 10;
  }
}

// ---------------------------------------------------------------------------
// Page elements
// ---------------------------------------------------------------------------
function drawTitleBar(title, subtitle, versionStr) {
  doc.rect(0, 0, PAGE_W, HEADER_H).fill(C.primaryBlue);
  doc.fillColor(C.white).font('Helvetica-Bold').fontSize(22)
     .text(title, MARGIN, 14, { width: USABLE_W - 110, lineBreak: false });
  doc.fillColor(C.white).font('Helvetica').fontSize(11)
     .text(subtitle, MARGIN, 42, { width: USABLE_W - 110, lineBreak: false });
  doc.fillColor(C.white).font('Helvetica').fontSize(9)
     .text(versionStr, PAGE_W - MARGIN - 130, 28, { width: 130, align: 'right', lineBreak: false });

  // Bottom edge accent line
  doc.rect(0, HEADER_H - 4, PAGE_W, 4).fill(C.lightGray);
  y = HEADER_H + 16;
}

function drawPageHeader(pageTitle) {
  // Thin blue bar at top of continuation pages
  doc.rect(0, 0, PAGE_W, 28).fill(C.primaryBlue);
  doc.fillColor(C.white).font('Helvetica-Bold').fontSize(10)
     .text(APP_TITLE + ' Architecture  |  ' + pageTitle, MARGIN, 9, { width: USABLE_W });
  y = 40;
}

function drawFooter(version, date) {
  // Separator
  doc.moveTo(MARGIN, PAGE_H - 36)
     .lineTo(PAGE_W - MARGIN, PAGE_H - 36)
     .strokeColor(C.lightGray).lineWidth(0.5).stroke();
  doc.fillColor(C.medGray).font('Helvetica').fontSize(8)
     .text(APP_TITLE + ' Architecture', MARGIN, PAGE_H - 27, { width: 180, lineBreak: false });
  doc.fillColor(C.medGray).font('Helvetica').fontSize(8)
     .text(`v${version}  ·  ${date}`, PAGE_W / 2 - 60, PAGE_H - 27, { width: 120, align: 'center', lineBreak: false });
  doc.fillColor(C.medGray).font('Helvetica').fontSize(8)
     .text(REPO_NAME || '', PAGE_W - MARGIN - 150, PAGE_H - 27, { width: 150, align: 'right', lineBreak: false });
}

function drawSectionHeading(text) {
  ensureSpace(44);
  y += 8;
  doc.fillColor(C.primaryBlue).font('Helvetica-Bold').fontSize(14)
     .text(text, MARGIN, y, { width: USABLE_W });
  y = doc.y + 4;
  doc.moveTo(MARGIN, y).lineTo(MARGIN + USABLE_W, y)
     .strokeColor(C.lightGray).lineWidth(0.5).stroke();
  y += 8;
}

function drawSubheading(text) {
  ensureSpace(24);
  y += 4;
  doc.fillColor(C.darkGray).font('Helvetica-Bold').fontSize(11)
     .text(text, MARGIN, y, { width: USABLE_W });
  y = doc.y + 6;
}

function drawParagraph(text) {
  if (!text || !text.trim()) return;
  ensureSpace(20);
  doc.fillColor(C.darkGray).font('Helvetica').fontSize(10)
     .text(text.trim(), MARGIN, y, { width: USABLE_W });
  y = doc.y + 6;
}

function drawBullets(items) {
  for (const item of items.slice(0, 20)) {
    const clean = item.replace(/\*\*/g, '').replace(/`/g, '').replace(/\[([^\]]+)\]\([^)]+\)/g, '$1').trim();
    if (!clean) continue;
    ensureSpace(14);
    doc.fillColor(C.primaryBlue).font('Helvetica-Bold').fontSize(10)
       .text('\u2022', MARGIN, y, { width: 12, lineBreak: false });
    doc.fillColor(C.darkGray).font('Helvetica').fontSize(10)
       .text(clean, MARGIN + 14, y, { width: USABLE_W - 14 });
    y = doc.y + 2;
  }
  y += 4;
}

function drawTable(headers, rows) {
  const colCount = headers.length;
  const colWid   = USABLE_W / colCount;
  const HDR_H    = 20;
  const ROW_H    = 18;

  ensureSpace(HDR_H + ROW_H * Math.min(rows.length, 3));

  const _drawHeader = (topY) => {
    doc.rect(MARGIN, topY, USABLE_W, HDR_H).fill(C.tblHeader);
    doc.rect(MARGIN, topY, USABLE_W, HDR_H)
       .strokeColor(C.lightGray).lineWidth(0.3).stroke();
    doc.fillColor(C.darkGray).font('Helvetica-Bold').fontSize(8);
    headers.forEach((h, i) => {
      const hClean = h.replace(/\*\*/g, '').replace(/`/g, '');
      doc.text(hClean, MARGIN + colWid * i + 4, topY + 5, {
        width: colWid - 8, height: HDR_H - 6, lineBreak: false, ellipsis: true,
      });
    });
    return topY + HDR_H;
  };

  y = _drawHeader(y);

  rows.forEach((row, ri) => {
    if (y + ROW_H > BOTTOM) {
      doc.addPage();
      y = MARGIN + 10;
      drawPageHeader('(continued)');
      y = _drawHeader(y);
    }
    const fill = ri % 2 === 0 ? C.white : C.tblStripe;
    doc.rect(MARGIN, y, USABLE_W, ROW_H).fill(fill);
    doc.rect(MARGIN, y, USABLE_W, ROW_H)
       .strokeColor(C.lightGray).lineWidth(0.3).stroke();
    doc.fillColor(C.darkGray).font('Helvetica').fontSize(8);
    row.forEach((cell, ci) => {
      const cellClean = (cell || '').replace(/\*\*/g, '').replace(/`/g, '').replace(/\[([^\]]+)\]\([^)]+\)/g, '$1');
      doc.text(cellClean, MARGIN + colWid * ci + 4, y + 4, {
        width: colWid - 8, height: ROW_H - 5, lineBreak: false, ellipsis: true,
      });
    });
    y += ROW_H;
  });

  y += 10;
}

function drawCodeBlock(text) {
  if (!text) return;
  const lines  = text.split('\n').slice(0, 40);
  const blockH = lines.length * 10 + 14;
  ensureSpace(blockH);

  doc.rect(MARGIN, y, USABLE_W, blockH).fill('#F8F8F8');
  doc.rect(MARGIN, y, 3, blockH).fill(C.primaryBlue);

  doc.fillColor(C.darkGray).font(MONO_FONT_NAME).fontSize(7.5);
  let lineY = y + 7;
  for (const line of lines) {
    const displayLine = MONO_UNICODE_CAPABLE ? line : sanitizeMono(line);
    doc.text(displayLine, MARGIN + 8, lineY, { width: USABLE_W - 16, lineBreak: false });
    lineY += 10;
  }
  y = lineY + 6;
}

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
    .map(l => l.replace(/^\s*[-*] /, '').trim())
    .filter(Boolean);
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
        headers = cells; started = true;
      } else if (cells.every(c => /^[-: ]+$/.test(c))) {
        // skip
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
    return s.replace(/\*\*/g, '').replace(/`/g, '').replace(/\[([^\]]+)\]\([^)]+\)/g, '$1');
  }
  return '';
}

// ---------------------------------------------------------------------------
// Section renderer
// ---------------------------------------------------------------------------
function renderSection(section) {
  const { num, title, content } = section;
  const bullets    = extractBullets(content);
  const tables     = extractTables(content);
  const codeBlocks = extractCodeBlocks(content);
  const para       = extractFirstParagraph(content);

  drawSectionHeading(`${num}. ${title}`);

  if (para) drawParagraph(para);
  if (bullets.length > 0) drawBullets(bullets);

  // Subsection headings
  const subLines = content.split('\n').filter(l => /^###? .+/.test(l.trim()));
  for (const sub of subLines) {
    drawSubheading(sub.replace(/^###? /, ''));
  }

  // Code blocks (diagrams / trees)
  for (const block of codeBlocks) drawCodeBlock(block);

  // Tables
  for (const tbl of tables) drawTable(tbl.headers, tbl.rows);
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
  const outputPath = outputIdx >= 0 ? args[outputIdx + 1] : 'Docs/DadABase-Architecture.pdf';

  if (!fs.existsSync(inputPath)) {
    console.error(`Error: Input file not found: ${inputPath}`);
    process.exit(1);
  }

  console.log(`Reading:  ${inputPath}`);
  const data = parseDocument(inputPath);
  console.log(`Parsed ${data.sections.length} sections — v${data.version} (${data.date})`);

  // Resolve display name and repo identifier from args, doc H1, or input filename
  APP_TITLE = titleIdx >= 0 ? args[titleIdx + 1]
    : (data.docTitle || path.basename(inputPath, path.extname(inputPath)).replace(/-?Architecture$/i, '').replace(/[-_]/g, ' ').trim() || 'Architecture');
  REPO_NAME = repoIdx >= 0 ? args[repoIdx + 1] : '';

  doc = new PDFDocument({
    size: 'LETTER',
    bufferPages: true,
    margins: { top: MARGIN, bottom: MARGIN, left: MARGIN, right: MARGIN },
    autoFirstPage: true,
  });

  // Register a Unicode-capable monospace font for code blocks (box-drawing chars)
  registerMonoFont(doc);

  const stream = fs.createWriteStream(outputPath);
  doc.pipe(stream);

  // First page title bar
  drawTitleBar(
    APP_TITLE + ' Architecture',
    'System Architecture Overview' + (REPO_NAME ? '  ·  ' + REPO_NAME : ''),
    `v${data.version}  ·  ${data.date}`
  );

  // Table of contents
  drawSectionHeading('Table of Contents');
  drawBullets(data.sections.map(s => `${s.num}. ${s.title}`));

  // Sections
  for (const section of data.sections) {
    renderSection(section);
  }

  // Footer on every page
  const range = doc.bufferedPageRange();
  for (let i = range.start; i < range.start + range.count; i++) {
    doc.switchToPage(i);
    drawFooter(data.version, data.date);
  }

  doc.end();

  await new Promise((resolve, reject) => {
    stream.on('finish', resolve);
    stream.on('error', reject);
  });

  const stats  = fs.statSync(outputPath);
  const sizeKB = Math.round(stats.size / 1024);
  const pages  = range.count;

  console.log('\n\u2705 PDF generated successfully');
  console.log(`   Output:   ${outputPath}`);
  console.log(`   Size:     ${sizeKB} KB`);
  console.log(`   Pages:    ${pages}`);
  console.log(`   Sections: ${data.sections.length}/15`);
}

main().catch(err => {
  console.error('PDF generation failed:', err.message);
  process.exit(1);
});
