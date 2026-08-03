<!DOCTYPE html>
<html lang="pt-BR">
<head>
<meta charset="UTF-8">
<meta name="viewport" content="width=device-width, initial-scale=1.0">
<title>Controle de Tokens</title>
<style>
  @import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&family=JetBrains+Mono:wght@500;700&display=swap');

  * { margin: 0; padding: 0; box-sizing: border-box; }

  :root {
    /* Localiza palette — built from brand green #78DE1F */
    --localiza-green: #78DE1F;
    --localiza-green-light: #8FE84A;
    --localiza-green-pale: #A8F06E;
    --bg-dark: #0C1A08;
    --bg-card: #142210;
    --bg-elevated: #1C2E16;
    --bg-input: rgba(120, 222, 31, 0.08);
    --border: rgba(120, 222, 31, 0.15);
    --border-hover: rgba(120, 222, 31, 0.35);
    --white: #ffffff;
    --white-90: rgba(255,255,255,0.9);
    --white-60: rgba(255,255,255,0.6);
    --white-20: rgba(255,255,255,0.2);
    --white-10: rgba(255,255,255,0.1);
    --white-05: rgba(255,255,255,0.05);
  }

  body {
    font-family: 'Inter', sans-serif;
    background: var(--bg-dark);
    color: var(--white);
    min-height: 100vh;
    display: flex;
    flex-direction: column;
    align-items: center;
    padding: 2rem 1rem;
  }

  header {
    text-align: center;
    margin-bottom: 2.5rem;
  }

  header h1 {
    font-size: 1.75rem;
    font-weight: 700;
    letter-spacing: -0.03em;
    margin-bottom: 0.35rem;
    color: var(--localiza-green);
  }

  header p {
    font-size: 0.85rem;
    color: var(--white-60);
    font-weight: 400;
  }

  .card {
    background: var(--bg-card);
    border: 1px solid var(--border);
    border-radius: 16px;
    padding: 2rem;
    width: 100%;
    max-width: 460px;
  }

  .selector-row {
    display: flex;
    gap: 0.75rem;
    margin-bottom: 2rem;
  }

  .field {
    flex: 1;
    display: flex;
    flex-direction: column;
    gap: 0.4rem;
  }

  .field label {
    font-size: 0.7rem;
    text-transform: uppercase;
    letter-spacing: 0.08em;
    color: var(--white-60);
    font-weight: 600;
  }

  .field select {
    appearance: none;
    background: var(--bg-input);
    border: 1px solid var(--border);
    border-radius: 10px;
    color: var(--white);
    font-family: 'Inter', sans-serif;
    font-size: 0.95rem;
    font-weight: 500;
    padding: 0.65rem 2rem 0.65rem 0.85rem;
    cursor: pointer;
    transition: border-color 0.2s, background 0.2s;
    background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='12' height='7'%3E%3Cpath d='M1 1l5 5 5-5' stroke='rgba(255,255,255,0.6)' stroke-width='1.5' fill='none'/%3E%3C/svg%3E");
    background-repeat: no-repeat;
    background-position: right 0.85rem center;
  }

  .field select:hover { border-color: var(--localiza-green); }
  .field select:focus { outline: none; border-color: var(--localiza-green); background-color: rgba(120,222,31,0.12); }
  .field select option { background: var(--bg-elevated); color: var(--white); }

  .divider {
    height: 1px;
    background: var(--border);
    margin-bottom: 1.75rem;
  }

  .results-top {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 1rem;
    margin-bottom: 1rem;
  }

  .result-box {
    background: var(--white-05);
    border: 1px solid var(--border);
    border-radius: 12px;
    padding: 1.1rem 1rem;
    text-align: center;
    transition: transform 0.2s, border-color 0.2s;
  }

  .result-box:hover {
    transform: translateY(-2px);
    border-color: var(--border-hover);
  }

  .result-box.highlight {
    background: linear-gradient(135deg, #1a3a0e, #254d14);
    border-color: var(--localiza-green);
    box-shadow: 0 0 24px rgba(120, 222, 31, 0.1);
  }

  .result-box .label {
    font-size: 0.68rem;
    text-transform: uppercase;
    letter-spacing: 0.08em;
    color: var(--white-60);
    font-weight: 600;
    margin-bottom: 0.5rem;
  }

  .result-box.highlight .label { color: var(--localiza-green-light); }

  .result-box .value {
    font-family: 'JetBrains Mono', monospace;
    font-size: 1.85rem;
    font-weight: 700;
    line-height: 1;
  }

  .result-box.highlight .value {
    font-size: 2.4rem;
    color: var(--localiza-green);
  }

  .result-box .unit {
    font-family: 'Inter', sans-serif;
    font-size: 0.75rem;
    font-weight: 500;
    color: var(--white-60);
    margin-top: 0.3rem;
  }

  .result-box.highlight .unit { color: var(--white-60); }

  /* Editable days úteis */
  .editable-row {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 0.6rem;
  }

  .edit-btn {
    width: 32px;
    height: 32px;
    border-radius: 8px;
    border: 1px solid var(--border);
    background: var(--bg-input);
    color: var(--white);
    font-size: 1.1rem;
    font-weight: 700;
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
    transition: background 0.2s, border-color 0.2s;
    line-height: 1;
    user-select: none;
  }

  .edit-btn:hover {
    background: rgba(120,222,31,0.15);
    border-color: var(--localiza-green);
  }

  .edit-btn:active {
    background: rgba(120,222,31,0.25);
  }

  .biz-input {
    width: 60px;
    text-align: center;
    background: transparent;
    border: none;
    color: var(--white);
    font-family: 'JetBrains Mono', monospace;
    font-size: 1.85rem;
    font-weight: 700;
    outline: none;
    -moz-appearance: textfield;
  }

  .biz-input::-webkit-outer-spin-button,
  .biz-input::-webkit-inner-spin-button {
    -webkit-appearance: none;
    margin: 0;
  }

  .biz-input:focus {
    border-bottom: 2px solid var(--localiza-green);
  }

  .edit-hint {
    font-size: 0.6rem;
    color: var(--white-60);
    text-align: center;
    margin-top: 0.25rem;
    font-style: italic;
  }

  .reset-btn {
    background: none;
    border: 1px solid var(--border);
    color: var(--white-60);
    font-family: 'Inter', sans-serif;
    font-size: 0.65rem;
    padding: 0.25rem 0.6rem;
    border-radius: 6px;
    cursor: pointer;
    transition: all 0.2s;
    margin-top: 0.4rem;
    display: none;
    text-transform: uppercase;
    letter-spacing: 0.05em;
    font-weight: 600;
  }

  .reset-btn:hover {
    border-color: var(--localiza-green);
    color: var(--white);
  }

  .reset-btn.visible { display: inline-block; }

  .progress-section {
    margin-top: 1.5rem;
  }

  .progress-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 0.5rem;
  }

  .progress-header span {
    font-size: 0.7rem;
    text-transform: uppercase;
    letter-spacing: 0.08em;
    color: var(--white-60);
    font-weight: 600;
  }

  .progress-bar {
    height: 8px;
    background: rgba(120,222,31,0.1);
    border-radius: 99px;
    overflow: hidden;
  }

  .progress-fill {
    height: 100%;
    background: linear-gradient(90deg, var(--localiza-green), var(--localiza-green-light));
    border-radius: 99px;
    transition: width 0.6s cubic-bezier(0.22, 1, 0.36, 1);
  }

  .calendar-section {
    margin-top: 1.75rem;
  }

  .calendar-section .section-title {
    font-size: 0.7rem;
    text-transform: uppercase;
    letter-spacing: 0.08em;
    color: var(--white-60);
    font-weight: 600;
    margin-bottom: 0.75rem;
  }

  .calendar-grid {
    display: grid;
    grid-template-columns: repeat(7, 1fr);
    gap: 4px;
  }

  .cal-header {
    font-size: 0.6rem;
    text-transform: uppercase;
    letter-spacing: 0.05em;
    color: var(--white-60);
    text-align: center;
    padding-bottom: 0.4rem;
    font-weight: 600;
  }

  .cal-day {
    aspect-ratio: 1;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 0.72rem;
    font-weight: 500;
    border-radius: 8px;
    color: var(--white-60);
  }

  .cal-day.business {
    background: rgba(120,222,31,0.15);
    color: var(--localiza-green-light);
    font-weight: 600;
  }

  .cal-day.weekend {
    color: rgba(255,255,255,0.15);
  }

  .cal-day.today {
    outline: 2px solid var(--localiza-green);
    outline-offset: -2px;
  }

  .legend {
    display: flex;
    gap: 1.25rem;
    margin-top: 0.75rem;
  }

  .legend-item {
    display: flex;
    align-items: center;
    gap: 0.35rem;
    font-size: 0.65rem;
    color: var(--white-60);
  }

  .legend-dot {
    width: 8px;
    height: 8px;
    border-radius: 3px;
  }

  .legend-dot.biz { background: rgba(120,222,31,0.3); }
  .legend-dot.wknd { background: var(--white-10); }
  .legend-dot.today-dot { outline: 2px solid var(--localiza-green); outline-offset: -1px; }

  footer {
    margin-top: 2rem;
    font-size: 0.7rem;
    color: rgba(255,255,255,0.15);
    text-align: center;
  }
</style>
</head>
<body>

<header>
  <h1>Controle de Tokens</h1>
  <p>Distribuição diária do orçamento mensal</p>
</header>

<div class="card">
  <div class="selector-row">
    <div class="field">
      <label>Mês</label>
      <select id="monthSelect"></select>
    </div>
    <div class="field">
      <label>Ano</label>
      <select id="yearSelect"></select>
    </div>
  </div>

  <div class="divider"></div>

  <div class="results-top">
    <div class="result-box">
      <div class="label">Dias no mês</div>
      <div class="value" id="totalDays">—</div>
    </div>
    <div class="result-box">
      <div class="label">Dias úteis</div>
      <div class="editable-row">
        <button class="edit-btn" id="btnMinus" title="Remover dia útil">−</button>
        <input type="number" class="biz-input" id="bizDaysInput" min="1" max="31" value="0">
        <button class="edit-btn" id="btnPlus" title="Adicionar dia útil">+</button>
      </div>
      <div class="edit-hint">ajuste para feriados</div>
      <button class="reset-btn" id="resetBtn">Resetar</button>
    </div>
  </div>

  <div class="result-box highlight">
    <div class="label">Limite diário de tokens</div>
    <div class="value" id="dailyPct">—</div>
    <div class="unit">do orçamento mensal por dia útil</div>
  </div>

  <div class="progress-section">
    <div class="progress-header">
      <span>Consumo ideal até hoje</span>
      <span id="progressLabel">—</span>
    </div>
    <div class="progress-bar">
      <div class="progress-fill" id="progressFill" style="width:0%"></div>
    </div>
  </div>

  <div class="calendar-section">
    <div class="section-title">Calendário do mês</div>
    <div class="calendar-grid" id="calendarGrid"></div>
    <div class="legend">
      <div class="legend-item"><div class="legend-dot biz"></div> Dia útil</div>
      <div class="legend-item"><div class="legend-dot wknd"></div> Fim de semana</div>
      <div class="legend-item"><div class="legend-dot today-dot"></div> Hoje</div>
    </div>
  </div>
</div>

<footer>100% ÷ dias úteis = limite diário</footer>

<script>
  const MONTHS = [
    'Janeiro','Fevereiro','Março','Abril','Maio','Junho',
    'Julho','Agosto','Setembro','Outubro','Novembro','Dezembro'
  ];

  const monthSelect = document.getElementById('monthSelect');
  const yearSelect = document.getElementById('yearSelect');
  const bizDaysInput = document.getElementById('bizDaysInput');
  const btnMinus = document.getElementById('btnMinus');
  const btnPlus = document.getElementById('btnPlus');
  const resetBtn = document.getElementById('resetBtn');

  let originalBizDays = 0;
  let userEdited = false;

  MONTHS.forEach((m, i) => {
    const opt = document.createElement('option');
    opt.value = i;
    opt.textContent = m;
    monthSelect.appendChild(opt);
  });

  const currentYear = new Date().getFullYear();
  for (let y = currentYear - 2; y <= currentYear + 3; y++) {
    const opt = document.createElement('option');
    opt.value = y;
    opt.textContent = y;
    yearSelect.appendChild(opt);
  }

  const today = new Date();
  monthSelect.value = today.getMonth();
  yearSelect.value = today.getFullYear();

  function getBusinessDays(year, month) {
    const daysInMonth = new Date(year, month + 1, 0).getDate();
    let count = 0;
    const days = [];
    for (let d = 1; d <= daysInMonth; d++) {
      const dow = new Date(year, month, d).getDay();
      const isBiz = dow !== 0 && dow !== 6;
      if (isBiz) count++;
      days.push({ day: d, dow, isBiz });
    }
    return { count, total: daysInMonth, days };
  }

  function getBusinessDaysPassed(year, month) {
    const now = new Date();
    if (now.getFullYear() !== year || now.getMonth() !== month) return null;
    let passed = 0;
    for (let d = 1; d <= now.getDate(); d++) {
      const dow = new Date(year, month, d).getDay();
      if (dow !== 0 && dow !== 6) passed++;
    }
    return passed;
  }

  function recalc() {
    const bizCount = parseInt(bizDaysInput.value) || 0;
    const pct = bizCount > 0 ? (100 / bizCount) : 0;
    document.getElementById('dailyPct').textContent = pct.toFixed(2) + '%';

    if (bizCount !== originalBizDays) {
      resetBtn.classList.add('visible');
      userEdited = true;
    } else {
      resetBtn.classList.remove('visible');
      userEdited = false;
    }

    const month = parseInt(monthSelect.value);
    const year = parseInt(yearSelect.value);
    const passed = getBusinessDaysPassed(year, month);
    const progressLabel = document.getElementById('progressLabel');
    const progressFill = document.getElementById('progressFill');

    if (passed !== null) {
      const consumed = (passed * pct);
      progressLabel.textContent = consumed.toFixed(1) + '% (' + passed + ' dias úteis)';
      progressFill.style.width = Math.min(consumed, 100) + '%';
    } else {
      progressLabel.textContent = 'Mês não atual';
      progressFill.style.width = '0%';
    }
  }

  function update() {
    const month = parseInt(monthSelect.value);
    const year = parseInt(yearSelect.value);
    const { count, total, days } = getBusinessDays(year, month);

    document.getElementById('totalDays').textContent = total;

    originalBizDays = count;
    bizDaysInput.value = count;
    bizDaysInput.max = total;
    userEdited = false;
    resetBtn.classList.remove('visible');

    recalc();

    const grid = document.getElementById('calendarGrid');
    grid.innerHTML = '';

    const dayNames = ['Dom','Seg','Ter','Qua','Qui','Sex','Sáb'];
    dayNames.forEach(name => {
      const el = document.createElement('div');
      el.className = 'cal-header';
      el.textContent = name;
      grid.appendChild(el);
    });

    const firstDow = new Date(year, month, 1).getDay();
    for (let i = 0; i < firstDow; i++) {
      grid.appendChild(document.createElement('div'));
    }

    const isCurrentMonth = today.getFullYear() === year && today.getMonth() === month;

    days.forEach(({ day, isBiz }) => {
      const el = document.createElement('div');
      el.className = 'cal-day';
      if (isBiz) el.classList.add('business');
      else el.classList.add('weekend');
      if (isCurrentMonth && day === today.getDate()) el.classList.add('today');
      el.textContent = day;
      grid.appendChild(el);
    });
  }

  monthSelect.addEventListener('change', update);
  yearSelect.addEventListener('change', update);

  btnMinus.addEventListener('click', () => {
    const val = parseInt(bizDaysInput.value) || 0;
    if (val > 1) {
      bizDaysInput.value = val - 1;
      recalc();
    }
  });

  btnPlus.addEventListener('click', () => {
    const val = parseInt(bizDaysInput.value) || 0;
    const max = parseInt(bizDaysInput.max) || 31;
    if (val < max) {
      bizDaysInput.value = val + 1;
      recalc();
    }
  });

  bizDaysInput.addEventListener('input', () => {
    let val = parseInt(bizDaysInput.value);
    const max = parseInt(bizDaysInput.max) || 31;
    if (isNaN(val) || val < 1) val = 1;
    if (val > max) val = max;
    bizDaysInput.value = val;
    recalc();
  });

  resetBtn.addEventListener('click', () => {
    bizDaysInput.value = originalBizDays;
    recalc();
  });

  update();
</script>
</body>
</html>
