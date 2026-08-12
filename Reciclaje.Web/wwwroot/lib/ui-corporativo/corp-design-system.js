/*!
 * @sistecsur/uicorporativo v1.0.4
 * Corporate Design System — JS
 * Build: 2026-03-06
 * License: UNLICENSED
 */
/*!
 * @corp/design-system — JavaScript v1.0.0
 * Componentes: Button, Combo, Table, Modal, Toast, Alert
 * Compatible: Vanilla JS, ASP.NET, Angular (wrapper), React (wrapper)
 */

(function (global, factory) {
  typeof exports === 'object' && typeof module !== 'undefined'
    ? module.exports = factory()
    : typeof define === 'function' && define.amd
      ? define(factory)
      : (global.CorporateDS = factory());
}(this, function () {
  'use strict';

  /* ──────────────────────────────────────────────
     UTILS
  ────────────────────────────────────────────── */
  function $(sel, ctx) { return (ctx || document).querySelector(sel); }
  function $$(sel, ctx) { return Array.from((ctx || document).querySelectorAll(sel)); }
  function emit(el, name, detail) {
    el.dispatchEvent(new CustomEvent(name, { bubbles: true, detail }));
  }

  /* ──────────────────────────────────────────────
     BUTTON
  ────────────────────────────────────────────── */
  const Button = {
    /**
     * Pone un botón en estado loading.
     * @param {HTMLElement|string} btn - Elemento o selector
     * @param {string} [text] - Texto mientras carga
     */
    setLoading(btn, text = 'Procesando…') {
      const el = typeof btn === 'string' ? $(btn) : btn;
      if (!el) return;
      el._origHTML = el.innerHTML;
      el._origDisabled = el.disabled;
      el.disabled = true;
      el.classList.add('corp-btn--loading');
      el.innerHTML = `<span class="corp-btn__spinner"></span><span>${text}</span>`;
    },

    /**
     * Restaura el botón desde estado loading.
     * @param {HTMLElement|string} btn
     */
    stopLoading(btn) {
      const el = typeof btn === 'string' ? $(btn) : btn;
      if (!el || !el._origHTML) return;
      el.innerHTML = el._origHTML;
      el.disabled = el._origDisabled;
      el.classList.remove('corp-btn--loading');
    }
  };

  /* ──────────────────────────────────────────────
     COMBO (Searchable Select)
  ────────────────────────────────────────────── */
  function Combo(element, options) {
    if (typeof element === 'string') element = $(element);
    if (!element) return;

    const opts = Object.assign({
      placeholder:   'Seleccionar…',
      searchPlaceholder: 'Buscar…',
      noResultsText: 'Sin resultados',
      searchable:    true,
      multiple:      false,
      onChange:      null
    }, options);

    // Build structure
    const wrap = document.createElement('div');
    wrap.className = 'corp-combo';

    let selectedValue = null;
    let selectedValues = [];

    // Parse original <select> if present, or use data-options
    let items = [];
    if (element.tagName === 'SELECT') {
      items = Array.from(element.options).map(o => ({
        value: o.value, label: o.text, selected: o.selected
      }));
      if (!opts.multiple) {
        selectedValue = element.value;
      } else {
        selectedValues = Array.from(element.selectedOptions).map(o => o.value);
      }
      element.style.display = 'none';
      element.parentNode.insertBefore(wrap, element);
      wrap.appendChild(element);
    } else {
      items = JSON.parse(element.dataset.options || '[]');
      element.parentNode.insertBefore(wrap, element);
      element.remove();
    }

    // ── Render ──
    function render() {
      wrap.innerHTML = '';
      if (element.tagName === 'SELECT') wrap.appendChild(element);

      const inputWrap = document.createElement('div');
      inputWrap.className = 'corp-combo__input-wrap';

      if (opts.multiple) {
        const tags = document.createElement('div');
        tags.className = 'corp-combo__tags';
        tags.tabIndex = 0;
        tags.setAttribute('role', 'button');
        tags.setAttribute('aria-haspopup', 'listbox');

        selectedValues.forEach(val => {
          const item = items.find(i => String(i.value) === String(val));
          if (!item) return;
          const tag = document.createElement('span');
          tag.className = 'corp-combo__tag';
          tag.innerHTML = `${item.label}<button class="corp-combo__tag-remove" data-val="${val}" aria-label="Quitar ${item.label}">×</button>`;
          tags.appendChild(tag);
        });

        if (!selectedValues.length) {
          const ph = document.createElement('span');
          ph.style.cssText = 'color:var(--corp-gray-400);font-size:var(--text-base);';
          ph.textContent = opts.placeholder;
          tags.appendChild(ph);
        }

        const arrow = document.createElement('span');
        arrow.className = 'corp-combo__arrow';
        arrow.innerHTML = '▼';
        inputWrap.appendChild(tags);
        inputWrap.appendChild(arrow);
      } else {
        const input = document.createElement('input');
        input.type = 'text';
        input.className = 'corp-combo__input';
        input.readOnly = true;
        input.placeholder = opts.placeholder;
        const sel = items.find(i => String(i.value) === String(selectedValue));
        input.value = sel ? sel.label : '';
        input.setAttribute('aria-haspopup', 'listbox');
        input.setAttribute('role', 'combobox');

        const arrow = document.createElement('span');
        arrow.className = 'corp-combo__arrow';
        arrow.innerHTML = '▼';

        inputWrap.appendChild(input);
        inputWrap.appendChild(arrow);
      }

      // Dropdown
      const dropdown = document.createElement('div');
      dropdown.className = 'corp-combo__dropdown';
      dropdown.setAttribute('role', 'listbox');

      if (opts.searchable) {
        const searchWrap = document.createElement('div');
        searchWrap.className = 'corp-combo__search-wrap';
        const searchInput = document.createElement('input');
        searchInput.type = 'text';
        searchInput.className = 'corp-combo__search';
        searchInput.placeholder = opts.searchPlaceholder;
        searchInput.setAttribute('aria-label', 'Buscar opciones');
        searchInput.addEventListener('input', function () {
          filterList(this.value.toLowerCase());
        });
        searchWrap.appendChild(searchInput);
        dropdown.appendChild(searchWrap);
      }

      const list = document.createElement('ul');
      list.className = 'corp-combo__list';

      function filterList(query) {
        const lis = Array.from(list.querySelectorAll('li'));
        let visible = 0;
        lis.forEach(li => {
          const match = li.dataset.label.toLowerCase().includes(query);
          li.style.display = match ? '' : 'none';
          if (match) visible++;
        });
        let noRes = list.querySelector('.corp-combo__no-results');
        if (!visible) {
          if (!noRes) {
            noRes = document.createElement('li');
            noRes.className = 'corp-combo__no-results';
            noRes.textContent = opts.noResultsText;
            list.appendChild(noRes);
          }
          noRes.style.display = '';
        } else if (noRes) {
          noRes.style.display = 'none';
        }
      }

      items.forEach(item => {
        const li = document.createElement('li');
        li.className = 'corp-combo__item';
        li.dataset.value = item.value;
        li.dataset.label = item.label;
        li.setAttribute('role', 'option');
        li.textContent = item.label;

        const isSel = opts.multiple
          ? selectedValues.includes(String(item.value))
          : String(item.value) === String(selectedValue);
        if (isSel) li.classList.add('selected');
        li.setAttribute('aria-selected', isSel);

        li.addEventListener('mousedown', function (e) {
          e.preventDefault();
          selectItem(item.value);
        });

        list.appendChild(li);
      });

      dropdown.appendChild(list);

      wrap.appendChild(inputWrap);
      wrap.appendChild(dropdown);

      // ── Events ──
      const trigger = opts.multiple
        ? inputWrap.querySelector('.corp-combo__tags')
        : inputWrap.querySelector('.corp-combo__input');

      trigger.addEventListener('click', toggleOpen);
      trigger.addEventListener('focus', () => trigger.classList && trigger.classList.add('focused'));
      trigger.addEventListener('blur', (e) => {
        if (!wrap.contains(e.relatedTarget)) {
          setTimeout(close, 150);
          trigger.classList && trigger.classList.remove('focused');
        }
      });

      if (opts.multiple) {
        inputWrap.querySelectorAll('.corp-combo__tag-remove').forEach(btn => {
          btn.addEventListener('mousedown', function (e) {
            e.stopPropagation();
            const val = this.dataset.val;
            selectedValues = selectedValues.filter(v => v !== val);
            syncNativeSelect();
            render();
            emit(wrap, 'corp:combo:change', { values: selectedValues });
            if (opts.onChange) opts.onChange(selectedValues);
          });
        });
      }
    }

    function toggleOpen() {
      wrap.classList.contains('open') ? close() : open();
    }
    function open() {
      wrap.classList.add('open');
      const searchInput = wrap.querySelector('.corp-combo__search');
      if (searchInput) setTimeout(() => searchInput.focus(), 50);
    }
    function close() {
      wrap.classList.remove('open');
      const searchInput = wrap.querySelector('.corp-combo__search');
      if (searchInput) searchInput.value = '';
      wrap.querySelectorAll('.corp-combo__item').forEach(li => li.style.display = '');
    }

    function selectItem(value) {
      if (opts.multiple) {
        const idx = selectedValues.indexOf(String(value));
        if (idx > -1) selectedValues.splice(idx, 1);
        else selectedValues.push(String(value));
        syncNativeSelect();
        render();
        emit(wrap, 'corp:combo:change', { values: selectedValues });
        if (opts.onChange) opts.onChange(selectedValues);
        open();
      } else {
        selectedValue = value;
        const item = items.find(i => String(i.value) === String(value));
        syncNativeSelect();
        render();
        close();
        emit(wrap, 'corp:combo:change', { value, label: item ? item.label : '' });
        if (opts.onChange) opts.onChange(value, item);
      }
    }

    function syncNativeSelect() {
      if (element.tagName !== 'SELECT') return;
      Array.from(element.options).forEach(o => {
        o.selected = opts.multiple
          ? selectedValues.includes(o.value)
          : o.value === String(selectedValue);
      });
      element.dispatchEvent(new Event('change', { bubbles: true }));
    }

    // Click outside to close
    document.addEventListener('click', function (e) {
      if (!wrap.contains(e.target)) close();
    });

    render();

    return {
      getValue: () => opts.multiple ? [...selectedValues] : selectedValue,
      setValue: (val) => { selectedValue = val; render(); },
      setValues: (vals) => { selectedValues = vals.map(String); render(); },
      destroy: () => { wrap.parentNode.replaceChild(element, wrap); }
    };
  }

  /* ──────────────────────────────────────────────
     TABLE
  ────────────────────────────────────────────── */
  function Table(element, options) {
    if (typeof element === 'string') element = $(element);
    if (!element) return;

    const wrap = element.closest('.corp-table-wrap') || element.parentElement;
    const opts = Object.assign({
      sortable:    true,
      searchable:  true,
      pageSize:    10,
      pageSizes:   [10, 25, 50, 100],
      selectable:  false
    }, options);

    let currentPage = 1;
    let sortCol = null;
    let sortDir = 'asc';
    let filterQuery = '';
    let allRows = [];
    let selectedRows = new Set();

    function init() {
      allRows = $$('tbody tr', element).filter(r => !r.classList.contains('corp-table__empty-row'));

      if (opts.sortable) initSort();
      if (opts.searchable) initSearch();
      if (opts.selectable) initSelect();
      renderPage();
    }

    function initSort() {
      $$('thead th.sortable', element).forEach(th => {
        const icon = document.createElement('span');
        icon.className = 'corp-table__sort-icon';
        icon.innerHTML = '⇅';
        th.appendChild(icon);
        th.addEventListener('click', () => {
          const col = th.dataset.col || th.cellIndex;
          if (sortCol === col) sortDir = sortDir === 'asc' ? 'desc' : 'asc';
          else { sortCol = col; sortDir = 'asc'; }
          sortRows();
          updateSortUI(th);
          renderPage();
        });
      });
    }

    function sortRows() {
      allRows.sort((a, b) => {
        const aVal = (a.cells[sortCol] || a.querySelector(`[data-col="${sortCol}"]`))?.textContent.trim() || '';
        const bVal = (b.cells[sortCol] || b.querySelector(`[data-col="${sortCol}"]`))?.textContent.trim() || '';
        const aNum = parseFloat(aVal.replace(/[^0-9.-]/g, ''));
        const bNum = parseFloat(bVal.replace(/[^0-9.-]/g, ''));
        const cmp = !isNaN(aNum) && !isNaN(bNum)
          ? aNum - bNum
          : aVal.localeCompare(bVal, 'es', { sensitivity: 'base' });
        return sortDir === 'asc' ? cmp : -cmp;
      });
    }

    function updateSortUI(activeTh) {
      $$('thead th.sortable', element).forEach(th => {
        th.classList.remove('sorted-asc', 'sorted-desc');
        const icon = th.querySelector('.corp-table__sort-icon');
        if (icon) icon.innerHTML = '⇅';
      });
      activeTh.classList.add(sortDir === 'asc' ? 'sorted-asc' : 'sorted-desc');
      const icon = activeTh.querySelector('.corp-table__sort-icon');
      if (icon) icon.innerHTML = sortDir === 'asc' ? '↑' : '↓';
    }

    function initSearch() {
      const searchInput = wrap.querySelector('.corp-table-search input');
      if (!searchInput) return;
      searchInput.addEventListener('input', function () {
        filterQuery = this.value.toLowerCase().trim();
        currentPage = 1;
        renderPage();
      });
    }

    function initSelect() {
      const headerCb = wrap.querySelector('thead .corp-table-checkbox');
      if (headerCb) {
        headerCb.addEventListener('change', function () {
          const rows = getVisibleRows();
          rows.forEach(r => {
            const cb = r.querySelector('.corp-table-checkbox');
            if (cb) {
              cb.checked = this.checked;
              this.checked ? selectedRows.add(r.dataset.id || r.rowIndex) : selectedRows.delete(r.dataset.id || r.rowIndex);
              r.classList.toggle('row--selected', this.checked);
            }
          });
          emit(element, 'corp:table:select', { selected: [...selectedRows] });
        });
      }
    }

    function getVisibleRows() {
      if (!filterQuery) return allRows;
      return allRows.filter(r => r.textContent.toLowerCase().includes(filterQuery));
    }

    function renderPage() {
      const visible = getVisibleRows();
      const total = visible.length;
      const pages = Math.ceil(total / opts.pageSize) || 1;
      currentPage = Math.min(currentPage, pages);
      const start = (currentPage - 1) * opts.pageSize;
      const end = start + opts.pageSize;

      const tbody = element.querySelector('tbody');

      // Remove all rows from DOM
      allRows.forEach(r => r.remove());

      // Remove empty state if present
      const emptyRow = tbody.querySelector('.corp-table__empty-row');
      if (emptyRow) emptyRow.remove();

      if (!total) {
        const tr = document.createElement('tr');
        tr.className = 'corp-table__empty-row';
        const td = document.createElement('td');
        td.colSpan = element.querySelector('thead tr')?.cells.length || 1;
        td.innerHTML = `
          <div class="corp-table__empty">
            <i class="bi bi-inbox corp-table__empty-icon"></i>
            <span class="corp-table__empty-text">Sin resultados</span>
            <span class="corp-table__empty-hint">Intenta con otros términos de búsqueda</span>
          </div>`;
        tr.appendChild(td);
        tbody.appendChild(tr);
      } else {
        visible.slice(start, end).forEach(r => tbody.appendChild(r));
      }

      updateFooter(start + 1, Math.min(end, total), total, pages);
      emit(element, 'corp:table:page', { page: currentPage, total, pageSize: opts.pageSize });
    }

    function updateFooter(from, to, total, pages) {
      const info = wrap.querySelector('.corp-table-info');
      if (info) {
        info.innerHTML = total
          ? `Mostrando <strong>${from}–${to}</strong> de <strong>${total}</strong> registros`
          : 'Sin registros';
      }
      renderPagination(pages);
    }

    function renderPagination(pages) {
      const container = wrap.querySelector('.corp-pagination');
      if (!container) return;
      container.innerHTML = '';

      const addBtn = (label, page, disabled, active) => {
        const btn = document.createElement('button');
        btn.className = 'corp-pagination__btn' + (active ? ' active' : '');
        btn.disabled = disabled;
        btn.innerHTML = label;
        btn.addEventListener('click', () => { currentPage = page; renderPage(); });
        container.appendChild(btn);
      };

      addBtn('«', 1, currentPage === 1, false);
      addBtn('‹', currentPage - 1, currentPage === 1, false);

      const range = getPaginationRange(currentPage, pages);
      range.forEach(p => {
        if (p === '…') {
          const el = document.createElement('span');
          el.className = 'corp-pagination__ellipsis';
          el.textContent = '…';
          container.appendChild(el);
        } else {
          addBtn(p, p, false, p === currentPage);
        }
      });

      addBtn('›', currentPage + 1, currentPage === pages, false);
      addBtn('»', pages, currentPage === pages, false);
    }

    function getPaginationRange(current, total) {
      if (total <= 7) return Array.from({ length: total }, (_, i) => i + 1);
      if (current <= 4) return [1, 2, 3, 4, 5, '…', total];
      if (current >= total - 3) return [1, '…', total-4, total-3, total-2, total-1, total];
      return [1, '…', current - 1, current, current + 1, '…', total];
    }

    // Per-page selector
    const perPage = wrap.querySelector('.corp-table-perpage select');
    if (perPage) {
      // Build options
      opts.pageSizes.forEach(s => {
        const o = document.createElement('option');
        o.value = s; o.textContent = s;
        if (s === opts.pageSize) o.selected = true;
        perPage.appendChild(o);
      });
      perPage.addEventListener('change', function () {
        opts.pageSize = parseInt(this.value);
        currentPage = 1;
        renderPage();
      });
    }

    init();

    return {
      refresh: init,
      goTo: (p) => { currentPage = p; renderPage(); },
      getSelected: () => [...selectedRows],
      setLoading: (v) => wrap.classList.toggle('loading', v)
    };
  }

  /* ──────────────────────────────────────────────
     MODAL
  ────────────────────────────────────────────── */
  const Modal = {
    _stack: [],

    open(idOrEl, options) {
      let backdrop = typeof idOrEl === 'string' ? $(`#${idOrEl}`) : idOrEl;

      if (!backdrop) {
        // Create dynamically
        const opts = Object.assign({
          title: 'Modal', size: '', variant: '',
          body: '', footer: '',
          onConfirm: null, onClose: null
        }, options);

        backdrop = document.createElement('div');
        backdrop.className = 'corp-modal-backdrop';
        backdrop.innerHTML = `
          <div class="corp-modal ${opts.size ? 'corp-modal--' + opts.size : ''}" role="dialog" aria-modal="true">
            <div class="corp-modal__header ${opts.variant ? 'corp-modal__header--' + opts.variant : ''}">
              <h3 class="corp-modal__title">${opts.title}</h3>
              <button class="corp-modal__close" aria-label="Cerrar">✕</button>
            </div>
            <div class="corp-modal__body">${opts.body}</div>
            ${opts.footer ? `<div class="corp-modal__footer">${opts.footer}</div>` : ''}
          </div>`;

        document.body.appendChild(backdrop);
        if (opts.onClose) {
          backdrop.dataset.hasCloseCallback = '1';
          backdrop._onClose = opts.onClose;
        }
        if (opts.onConfirm) {
          const confirmBtn = backdrop.querySelector('[data-action="confirm"]');
          if (confirmBtn) confirmBtn.addEventListener('click', opts.onConfirm);
        }
      }

      backdrop.style.display = 'flex';
      document.body.style.overflow = 'hidden';
      this._stack.push(backdrop);

      const closeBtn = backdrop.querySelector('.corp-modal__close');
      if (closeBtn) closeBtn.onclick = () => Modal.close(backdrop);

      backdrop.addEventListener('click', (e) => {
        if (e.target === backdrop) Modal.close(backdrop);
      });

      document.addEventListener('keydown', this._escHandler);
      emit(backdrop, 'corp:modal:open');
      return backdrop;
    },

    close(idOrEl) {
      let backdrop = typeof idOrEl === 'string' ? $(`#${idOrEl}`) : idOrEl;
      if (!backdrop && this._stack.length) backdrop = this._stack[this._stack.length - 1];
      if (!backdrop) return;

      const modal = backdrop.querySelector('.corp-modal');
      if (modal) {
        modal.style.animation = 'corp-modal-in 0.25s var(--ease-out) reverse both';
        backdrop.style.animation = 'corp-fade-in 0.2s var(--ease-out) reverse both';
      }

      setTimeout(() => {
        backdrop.style.display = 'none';
        if (backdrop._onClose) backdrop._onClose();
        this._stack = this._stack.filter(b => b !== backdrop);
        if (!this._stack.length) document.body.style.overflow = '';
        // Remove if dynamically created
        if (!backdrop.id) backdrop.remove();
      }, 250);

      emit(backdrop, 'corp:modal:close');
    },

    _escHandler(e) {
      if (e.key === 'Escape') Modal.close();
    },

    confirm(opts) {
      return new Promise(resolve => {
        const backdrop = Modal.open(null, {
          title:   opts.title   || 'Confirmar',
          body:    opts.message || '¿Deseas continuar?',
          variant: opts.variant || 'danger',
          size:    'sm',
          footer: `
            <button class="corp-btn corp-btn--ghost" data-action="cancel">Cancelar</button>
            <button class="corp-btn corp-btn--${opts.variant || 'danger'}" data-action="confirm">${opts.confirmText || 'Confirmar'}</button>
          `,
          onClose: () => resolve(false)
        });

        backdrop.querySelector('[data-action="confirm"]').addEventListener('click', () => {
          Modal.close(backdrop);
          resolve(true);
        });
        backdrop.querySelector('[data-action="cancel"]').addEventListener('click', () => {
          Modal.close(backdrop);
          resolve(false);
        });
      });
    }
  };

  /* ──────────────────────────────────────────────
     TOAST
  ────────────────────────────────────────────── */
  const Toast = {
    _container: null,

    _getContainer() {
      if (!this._container) {
        this._container = $('#corp-toast-container') ||
          document.querySelector('.corp-toast-container');
        if (!this._container) {
          this._container = document.createElement('div');
          this._container.className = 'corp-toast-container';
          this._container.id = 'corp-toast-container';
          document.body.appendChild(this._container);
        }
      }
      return this._container;
    },

    show(options) {
      if (typeof options === 'string') options = { message: options };

      const opts = Object.assign({
        type:     'info',      // success | danger | warning | info
        title:    '',
        message:  '',
        duration: 4000,
        closable: true
      }, options);

      const icons = {
        success: 'bi bi-check-circle-fill',
        danger:  'bi bi-x-circle-fill',
        warning: 'bi bi-exclamation-triangle-fill',
        info:    'bi bi-info-circle-fill'
      };

      const toast = document.createElement('div');
      toast.className = `corp-toast corp-toast--${opts.type}`;
      toast.setAttribute('role', 'alert');
      toast.innerHTML = `
        <i class="${icons[opts.type] || icons.info} corp-toast__icon"></i>
        <div class="corp-toast__body">
          ${opts.title ? `<span class="corp-toast__title">${opts.title}</span>` : ''}
          ${opts.message ? `<span class="corp-toast__msg">${opts.message}</span>` : ''}
        </div>
        ${opts.closable ? `<button class="corp-toast__close" aria-label="Cerrar">✕</button>` : ''}
      `;

      const closeBtn = toast.querySelector('.corp-toast__close');
      if (closeBtn) closeBtn.addEventListener('click', () => this._dismiss(toast));

      this._getContainer().appendChild(toast);

      if (opts.duration > 0) {
        setTimeout(() => this._dismiss(toast), opts.duration);
      }

      return toast;
    },

    _dismiss(toast) {
      toast.classList.add('hiding');
      setTimeout(() => toast.remove(), 300);
    },

    success: (msg, title) => Toast.show({ type: 'success', message: msg, title }),
    danger:  (msg, title) => Toast.show({ type: 'danger',  message: msg, title }),
    warning: (msg, title) => Toast.show({ type: 'warning', message: msg, title }),
    info:    (msg, title) => Toast.show({ type: 'info',    message: msg, title })
  };

  /* ──────────────────────────────────────────────
     ALERT (inline dismissible)
  ────────────────────────────────────────────── */
  const Alert = {
    init(container) {
      $$(container ? `${container} [data-dismiss="alert"]` : '[data-dismiss="alert"]')
        .forEach(btn => {
          btn.addEventListener('click', () => {
            const alert = btn.closest('.corp-alert');
            if (alert) {
              alert.style.transition = 'opacity 0.25s, transform 0.25s';
              alert.style.opacity = '0';
              alert.style.transform = 'translateY(-6px)';
              setTimeout(() => alert.remove(), 260);
            }
          });
        });
    }
  };

  /* ──────────────────────────────────────────────
     LOADING OVERLAY
  ────────────────────────────────────────────── */
  const Loading = {
    _el: null,

    show(message = 'Procesando…') {
      if (!this._el) {
        this._el = document.createElement('div');
        this._el.className = 'corp-loading-overlay';
        this._el.innerHTML = `
          <div class="corp-spinner corp-spinner--xl corp-spinner--white"></div>
          <div class="corp-loading-overlay__msg"></div>`;
        document.body.appendChild(this._el);
      }
      this._el.querySelector('.corp-loading-overlay__msg').textContent = message;
      this._el.style.display = 'flex';
      document.body.style.overflow = 'hidden';
    },

    hide() {
      if (this._el) this._el.style.display = 'none';
      document.body.style.overflow = '';
    },

    async wrap(fn, message) {
      this.show(message);
      try { return await fn(); }
      finally { this.hide(); }
    }
  };

  /* ──────────────────────────────────────────────
     AUTO-INIT on DOMContentLoaded
  ────────────────────────────────────────────── */
  document.addEventListener('DOMContentLoaded', () => {
    // Auto-init combos
    $$('[data-corp-combo]').forEach(el => new Combo(el));

    // Auto-init tables
    $$('[data-corp-table]').forEach(el => {
      const opts = {};
      try { Object.assign(opts, JSON.parse(el.dataset.corpTable || '{}')); } catch(e) {}
      new Table(el, opts);
    });

    // Auto-init alerts
    Alert.init();
  });

  /* ──────────────────────────────────────────────
     PUBLIC API
  ────────────────────────────────────────────── */
  return { Button, Combo, Table, Modal, Toast, Alert, Loading };

}));

