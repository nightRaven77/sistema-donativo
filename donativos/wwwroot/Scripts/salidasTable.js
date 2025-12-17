(function (window, document, $) {
    'use strict';

    // TableManager: sorting + pagination + actions
    const TableManager = {
        data: [],
        table: null,
        tbody: null,
        pageSize: 10,
        page: 1,
        sortKey: null,
        sortDir: 'asc',
        editHandler: null,
        deleteHandler: null,

        init: function (tableId) {
            this.table = document.getElementById(tableId);
            if (!this.table) return;
            this.tbody = this.table.querySelector('tbody');

            // attach sortable headers
            const headers = this.table.querySelectorAll('th[data-sortable="true"]');
            headers.forEach(h => {
                h.style.cursor = 'pointer';
                h.addEventListener('click', () => {
                    const key = h.getAttribute('data-key');
                    if (this.sortKey === key) {
                        this.sortDir = this.sortDir === 'asc' ? 'desc' : 'asc';
                    } else {
                        this.sortKey = key;
                        this.sortDir = 'asc';
                    }
                    this.page = 1;
                    this.render();
                    this.updateSortIndicators();
                });
            });

            // page size select
            const pageSizeSelect = document.getElementById('pageSizeSelect');
            if (pageSizeSelect) {
                pageSizeSelect.addEventListener('change', (e) => {
                    this.pageSize = parseInt(e.target.value, 10);
                    this.page = 1;
                    this.render();
                });
            }

            // delegate action buttons (edit/delete)
            this.tbody.addEventListener('click', (e) => {
                const btn = e.target.closest('button[data-action]');
                if (!btn) return;
                const id = btn.getAttribute('data-id');
                const action = btn.getAttribute('data-action');
                if (action === 'edit') {
                    this._handleEdit(id, btn);
                } else if (action === 'delete') {
                    this._handleDelete(id, btn);
                }
            }, true);
        },

        setData: function (arr) {
            this.data = Array.isArray(arr) ? arr.slice() : [];
            this.page = 1;
            this.render();
        },

        registerEditHandler: function (fn) {
            this.editHandler = fn;
        },

        registerDeleteHandler: function (fn) {
            this.deleteHandler = fn;
        },

        _handleEdit: function (id, btn) {
            if (typeof this.editHandler === 'function') {
                this.editHandler(id, btn);
                return;
            }
            // default: try salidasUtils.editaRegistro(id) else dispatch event
            if (window.salidasUtils && typeof window.salidasUtils.editaRegistro === 'function') {

                try { window.salidasUtils.editaRegistro(id, window.aValue); } catch (e) { console.error(e); }
            } else {
                document.dispatchEvent(new CustomEvent('salidas:edit', { detail: { id } }));
            }
        },

        _handleDelete: function (id, btn) {
            if (typeof this.deleteHandler === 'function') {
                this.deleteHandler(id, btn);
                return;
            }
            // default: try salidasUtils.eliminaRegistro(id) else dispatch event
            if (window.salidasUtils && typeof window.salidasUtils.eliminaRegistro === 'function') {
                try { window.salidasUtils.eliminaRegistro(id, window.aValue); } catch (e) { console.error(e); }
            } else {
                document.dispatchEvent(new CustomEvent('salidas:delete', { detail: { id } }));
            }
        },

        sortData: function (arr) {
            if (!this.sortKey) return arr;
            const key = this.sortKey;
            const dir = this.sortDir === 'asc' ? 1 : -1;
            return arr.sort((a, b) => {
                const va = a[key] ?? '';
                const vb = b[key] ?? '';
                const na = parseFloat(va);
                const nb = parseFloat(vb);
                if (!isNaN(na) && !isNaN(nb)) {
                    return (na - nb) * dir;
                }
                return String(va).localeCompare(String(vb), undefined, { numeric: true }) * dir;
            });
        },

        render: function () {
            const sorted = this.sortData(this.data.slice());
            const total = sorted.length;
            const start = (this.page - 1) * this.pageSize;
            const end = start + this.pageSize;
            const pageItems = sorted.slice(start, end);

            if (!this.tbody) return;
            this.tbody.innerHTML = '';

            pageItems.forEach(item => {
                const tr = document.createElement('tr');

                // Use RegistroSalidaID (name used in controller) or fallback to id
                const id = item.registroSalidaID ?? item.id ?? '';

                // action buttons: Edit / Delete
                const btnEdit = `<button class="btn btn-sm btn-outline-primary me-1" data-action="edit" data-id="${id}" title="Editar"><i class="bi bi-pencil"></i> Edit</button>`;
                const btnDelete = `<button class="btn btn-sm btn-outline-danger" data-action="delete" data-id="${id}" title="Eliminar"><i class="bi bi-trash"></i> Del</button>`;

                tr.innerHTML = `
                    <td class="text-nowrap">${btnEdit} ${btnDelete}</td>
                    <td></td>
                    <td>${escapeHtml(item.folio ?? '')}</td>
                    <td>${escapeHtml(item.fecha ?? '')}</td>
                    <td>${escapeHtml(item.nombreConductor ?? '')}</td>
                    <td>${escapeHtml(item.telefonoConductor ?? '')}</td>
                    <td>${escapeHtml(item.nombreArticulo ?? '')}</td>
                    <td>${escapeHtml(item.cantidad ?? '')}</td>
                    <td>${escapeHtml(item.nombreDestino ?? '')}</td>
                    <td>${escapeHtml(item.placaVehiculo ?? '')}</td>
                    <td>${escapeHtml(item.localidad ?? '')}</td>
                    <td>${escapeHtml(item.observaciones ?? '')}</td>
                    <td>${escapeHtml(item.dependencia ?? '')}</td>
                    <td>${escapeHtml(item.salida ?? '')}</td>
                `;
                this.tbody.appendChild(tr);
            });

            this.renderPagination(total);
            this.updateSortIndicators();
        },

        renderPagination: function (totalItems) {
            const totalPages = Math.max(1, Math.ceil(totalItems / this.pageSize));
            const container = document.getElementById('registrosPagination');
            if (!container) return;
            container.innerHTML = '';

            const makePageItem = (label, page, disabled = false, active = false) => {
                const li = document.createElement('li');
                li.className = 'page-item' + (disabled ? ' disabled' : '') + (active ? ' active' : '');
                const a = document.createElement('a');
                a.className = 'page-link';
                a.href = '#';
                a.textContent = label;
                a.addEventListener('click', (e) => {
                    e.preventDefault();
                    if (!disabled) {
                        this.page = page;
                        this.render();
                    }
                });
                li.appendChild(a);
                return li;
            };

            container.appendChild(makePageItem('«', Math.max(1, this.page - 1), this.page === 1));
            const maxButtons = 7;
            let start = Math.max(1, this.page - Math.floor(maxButtons / 2));
            let end = Math.min(totalPages, start + maxButtons - 1);
            if (end - start < maxButtons - 1) start = Math.max(1, end - maxButtons + 1);
            for (let p = start; p <= end; p++) {
                container.appendChild(makePageItem(p, p, false, p === this.page));
            }
            container.appendChild(makePageItem('»', Math.min(totalPages, this.page + 1), this.page === totalPages));
        },

        updateSortIndicators: function () {
            const headers = this.table.querySelectorAll('th[data-sortable="true"]');
            headers.forEach(h => {
                const key = h.getAttribute('data-key');
                const indicator = h.querySelector('.sort-indicator');
                if (indicator) {
                    indicator.textContent = (key === this.sortKey) ? (this.sortDir === 'asc' ? '▲' : '▼') : '';
                }
            });
        }
    };

    // small utility to avoid XSS when injecting strings
    function escapeHtml(str) {
        if (str === null || str === undefined) return '';
        return String(str)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    }

    // expose
    window.SalidasTable = TableManager;

    // initialize automatically if table exists when DOM ready
    document.addEventListener('DOMContentLoaded', function () {
        TableManager.init('registros');
    });

})(window, document, jQuery);