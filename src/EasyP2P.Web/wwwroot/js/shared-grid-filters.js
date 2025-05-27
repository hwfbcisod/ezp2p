// shared-grid-filters.js

function initializeGridFilters(config) {
    const table = document.getElementById(config.tableId);
    if (!table) {
        console.error(`Table with ID "${config.tableId}" not found.`);
        return;
    }
    const tbody = table.querySelector('tbody');
    if (!tbody) {
        console.error(`Tbody not found in table "${config.tableId}".`);
        return;
    }

    const tableRows = Array.from(tbody.querySelectorAll('tr'));
    const totalRowCount = tableRows.length;

    const filterContainer = document.getElementById(config.filterContainerId);
    const filterInputs = filterContainer ? Array.from(filterContainer.querySelectorAll('.filter-input')) : [];

    const filtersToggleBtn = document.getElementById(config.filterToggleBtnId);
    const clearFiltersBtn = document.getElementById(config.clearFiltersBtnId);
    const applyFiltersBtn = document.getElementById(config.applyFiltersBtnId);
    const resetFiltersBtn = document.getElementById(config.resetFiltersBtnId);

    const sortButtons = Array.from(table.querySelectorAll('.sort-btn'));
    const statusCards = config.statusCardClass ? Array.from(document.querySelectorAll('.' + config.statusCardClass)) : [];

    // UI elements for counts - ensure these exist or are handled gracefully
    const filteredCountSpan = filterContainer ? filterContainer.querySelector('.filtered-count') : null;
    const totalRowCountSpan = filterContainer ? filterContainer.querySelector('.total-row-count') : null;
    const mainGridTotalCountSpan = document.querySelector('.total-grid-count');
    const mainGridFilteredBadge = document.querySelector('.filtered-grid-badge');


    let currentSort = { column: null, direction: 'asc' };
    // Correctly initialize Bootstrap Collapse instance
    const bsCollapse = (filterContainer && bootstrap.Collapse) ? new bootstrap.Collapse(filterContainer, { toggle: false }) : null;


    function updateCounts(visibleCount) {
        if (filteredCountSpan) filteredCountSpan.textContent = visibleCount;
        if (totalRowCountSpan) totalRowCountSpan.textContent = totalRowCount; // Should always be total from page load
        if (mainGridTotalCountSpan) mainGridTotalCountSpan.textContent = visibleCount;
    }

    function updateFilterButtonStates(hasActiveFilters) {
        if (clearFiltersBtn) clearFiltersBtn.style.display = hasActiveFilters ? 'inline-block' : 'none';
        if (filtersToggleBtn && filterContainer) { // Added null check for filterContainer
            if (filterContainer.classList.contains('show')) {
                filtersToggleBtn.innerHTML = `<i class="bi bi-funnel${hasActiveFilters ? '-fill' : ''}"></i> Filters & Search (Open${hasActiveFilters ? ', Active' : ''})`;
            } else {
                filtersToggleBtn.innerHTML = `<i class="bi bi-funnel${hasActiveFilters ? '-fill' : ''}"></i> Filters & Search ${hasActiveFilters ? '(Active)' : ''}`;
            }
        }
        if (mainGridFilteredBadge) mainGridFilteredBadge.style.display = hasActiveFilters ? 'inline-block' : 'none';
    }

    function getFilterValues() {
        const values = {};
        filterInputs.forEach(input => {
            const key = input.dataset.filterKey || input.id; // Use data-filter-key if present
            if (input.type === 'checkbox') {
                values[key] = input.checked;
            } else {
                values[key] = input.value.trim().toLowerCase();
            }
        });
        return values;
    }

    function applyAllFilters() {
        const filters = getFilterValues();
        let visibleCount = 0;
        let hasActiveFilters = Object.values(filters).some(val => {
            if (typeof val === 'boolean') return val; // For checkboxes
            return val !== '';
        });


        tableRows.forEach(row => {
            let isVisible = true;
            for (const key in filters) {
                const filterValue = filters[key];
                // Skip if filterValue is empty string for text/select, or false for checkbox
                if ((typeof filterValue === 'string' && filterValue === '') || (typeof filterValue === 'boolean' && !filterValue)) {
                    continue;
                }

                let rowValue = row.dataset[key];

                if (rowValue === undefined) {
                    // Fallback for common names if data-filter-key wasn't specific enough
                    if (key === 'status' && row.dataset.status) rowValue = row.dataset.status;
                    else if (key === 'priority' && row.dataset.priority) rowValue = row.dataset.priority;
                    else if (key === 'item' && row.dataset.item) rowValue = row.dataset.item;
                    else if (key === 'name' && row.dataset.name) rowValue = row.dataset.name;
                    // Add more fallbacks as needed
                }

                if (rowValue === undefined) continue; // Skip if data attribute for filter key doesn't exist

                rowValue = rowValue.toLowerCase(); // Ensure rowValue is also toLowerCase for comparison

                if (key.endsWith('Min')) {
                    const actualKey = key.slice(0, -3);
                    const rowActualValue = parseFloat(row.dataset[actualKey]);
                    if (isNaN(rowActualValue) || rowActualValue < parseFloat(filterValue)) isVisible = false;
                } else if (key.endsWith('Max')) {
                    const actualKey = key.slice(0, -3);
                    const rowActualValue = parseFloat(row.dataset[actualKey]);
                    if (isNaN(rowActualValue) || rowActualValue > parseFloat(filterValue)) isVisible = false;
                } else if (key === 'dateFrom') {
                    if (row.dataset.date && row.dataset.date < filterValue) isVisible = false;
                    else if (!row.dataset.date && filterValue !== '') isVisible = false; // Hide if filter is set but row has no date
                } else if (key === 'dateTo') {
                    if (row.dataset.date && row.dataset.date > filterValue) isVisible = false;
                    else if (!row.dataset.date && filterValue !== '') isVisible = false; // Hide if filter is set but row has no date
                } else if (key === 'deliveryDate') { // Ensure this key matches data-filter-key
                    if (row.dataset.deliveryDate) { // Ensure row has delivery-date attribute
                        const deliveryDate = new Date(row.dataset.deliveryDate);
                        const today = new Date();
                        today.setHours(0, 0, 0, 0);

                        switch (filterValue) {
                            case 'overdue': if (deliveryDate >= today) isVisible = false; break;
                            case 'thisweek':
                                const currentDay = today.getDay(); // 0 (Sun) - 6 (Sat)
                                const firstDayOfWeek = new Date(today);
                                firstDayOfWeek.setDate(today.getDate() - currentDay + (currentDay === 0 ? -6 : 1)); // Adjust to Monday
                                const lastDayOfWeek = new Date(firstDayOfWeek);
                                lastDayOfWeek.setDate(firstDayOfWeek.getDate() + 6);
                                if (deliveryDate < firstDayOfWeek || deliveryDate > lastDayOfWeek) isVisible = false;
                                break;
                            case 'nextweek':
                                const firstDayOfNextWeek = new Date(today);
                                firstDayOfNextWeek.setDate(today.getDate() - today.getDay() + 8); // Start of next week (Monday)
                                const lastDayOfNextWeek = new Date(firstDayOfNextWeek);
                                lastDayOfNextWeek.setDate(firstDayOfNextWeek.getDate() + 6);
                                if (deliveryDate < firstDayOfNextWeek || deliveryDate > lastDayOfNextWeek) isVisible = false;
                                break;
                            case 'thismonth':
                                const firstDayOfMonth = new Date(today.getFullYear(), today.getMonth(), 1);
                                const lastDayOfMonth = new Date(today.getFullYear(), today.getMonth() + 1, 0);
                                if (deliveryDate < firstDayOfMonth || deliveryDate > lastDayOfMonth) isVisible = false;
                                break;
                        }
                    } else if (filterValue !== '') { // If filter is set but row has no delivery date
                        isVisible = false;
                    }
                }
                else { // Default: string contains
                    if (!rowValue.includes(filterValue)) {
                        isVisible = false;
                    }
                }
                if (!isVisible) break; // Stop checking other filters for this row
            }

            row.style.display = isVisible ? '' : 'none';
            if (isVisible) visibleCount++;
        });

        updateCounts(visibleCount);
        updateFilterButtonStates(hasActiveFilters);
    }

    function clearAllFilters() {
        filterInputs.forEach(input => {
            if (input.type === 'checkbox') input.checked = false;
            else input.value = '';
        });
        statusCards.forEach(card => card.classList.remove('active-status-card'));
        applyAllFilters(); // This will update counts and button states
    }

    if (applyFiltersBtn) {
        applyFiltersBtn.addEventListener('click', applyAllFilters);
    }

    if (resetFiltersBtn) {
        resetFiltersBtn.addEventListener('click', clearAllFilters);
    }
    if (clearFiltersBtn) {
        clearFiltersBtn.addEventListener('click', clearAllFilters);
    }

    let filterTimeout;
    filterInputs.forEach(input => {
        input.addEventListener('input', () => {
            clearTimeout(filterTimeout);
            filterTimeout = setTimeout(applyAllFilters, 300); // Debounce filtering
        });
        // For select elements, apply filter immediately on change
        if (input.tagName.toLowerCase() === 'select') {
            input.addEventListener('change', () => {
                clearTimeout(filterTimeout); // Clear any pending timeout
                applyAllFilters();
            });
        }
    });

    if (filtersToggleBtn && filterContainer) {
        filterContainer.addEventListener('shown.bs.collapse', function () {
            const hasActiveFilters = Object.values(getFilterValues()).some(val => val !== '' && val !== false);
            filtersToggleBtn.innerHTML = `<i class="bi bi-funnel${hasActiveFilters ? '-fill' : ''}"></i> Filters & Search (Open${hasActiveFilters ? ', Active' : ''})`;
        });
        filterContainer.addEventListener('hidden.bs.collapse', function () {
            const hasActiveFilters = Object.values(getFilterValues()).some(val => val !== '' && val !== false);
            filtersToggleBtn.innerHTML = `<i class="bi bi-funnel${hasActiveFilters ? '-fill' : ''}"></i> Filters & Search ${hasActiveFilters ? '(Active)' : ''}`;
        });
    }

    statusCards.forEach(card => {
        card.addEventListener('click', function () {
            const statusFilterVal = this.dataset.statusFilterValue;
            const priorityFilterVal = this.dataset.priorityFilterValue;

            statusCards.forEach(c => c.classList.remove('active-status-card'));
            this.classList.add('active-status-card');

            if (bsCollapse) bsCollapse.show(); // Corrected typo here

            // Reset other input fields before applying card filter
            filterInputs.forEach(input => {
                const key = input.dataset.filterKey || input.id;
                if (key !== 'status' && key !== 'priority') {
                    if (input.type === 'checkbox') input.checked = false;
                    else input.value = '';
                }
            });

            const statusInput = filterInputs.find(fi => (fi.dataset.filterKey || fi.id) === 'status');
            const priorityInput = filterInputs.find(fi => (fi.dataset.filterKey || fi.id) === 'priority');

            if (statusFilterVal !== undefined && statusInput) {
                statusInput.value = statusFilterVal;
                if (priorityInput) priorityInput.value = '';
            }
            if (priorityFilterVal !== undefined && priorityInput) {
                priorityInput.value = priorityFilterVal;
                if (statusInput) statusInput.value = '';
            }

            applyAllFilters();
        });
        card.addEventListener('keydown', function (e) {
            if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                this.click();
            }
        });
    });

    sortButtons.forEach(button => {
        button.addEventListener('click', function () {
            const column = this.dataset.column;
            // const currentDirection = this.dataset.direction || 'asc'; // Not needed with currentSort
            const newDirection = currentSort.column === column && currentSort.direction === 'asc' ? 'desc' : 'asc';

            sortButtons.forEach(btn => {
                btn.classList.remove('active');
                btn.innerHTML = '<i class="bi bi-arrow-up-down"></i>';
                // btn.dataset.direction = 'asc'; // Reset only if not the current sort column
            });

            this.classList.add('active');
            this.innerHTML = newDirection === 'asc' ? '<i class="bi bi-arrow-up"></i>' : '<i class="bi bi-arrow-down"></i>';
            this.dataset.direction = newDirection; // Update the button's direction state
            currentSort = { column, direction: newDirection };

            sortTable(column, newDirection);
        });
    });

    function sortTable(column, direction) {
        const rowsArray = Array.from(tableRows);

        rowsArray.sort((a, b) => {
            let aValue = a.dataset[column];
            let bValue = b.dataset[column];

            // Handle potentially undefined values by treating them as empty strings or a default low/high value
            aValue = aValue === undefined ? '' : aValue;
            bValue = bValue === undefined ? '' : bValue;

            const aNum = parseFloat(aValue);
            const bNum = parseFloat(bValue);

            if (!isNaN(aNum) && !isNaN(bNum) && (column.includes('quantity') || column.includes('price') || column.includes('id') || column.includes('rating'))) {
                aValue = aNum;
                bValue = bNum;
            } else if (column === 'date' || column === 'deliveryDate') {
                const dateA = aValue ? new Date(aValue) : null;
                const dateB = bValue ? new Date(bValue) : null;

                if (!dateA && !dateB) return 0;
                if (!dateA) return direction === 'asc' ? 1 : -1;
                if (!dateB) return direction === 'asc' ? -1 : 1;

                aValue = dateA;
                bValue = dateB;
            } else {
                aValue = aValue.toLowerCase();
                bValue = bValue.toLowerCase();
            }

            if (aValue < bValue) return direction === 'asc' ? -1 : 1;
            if (aValue > bValue) return direction === 'asc' ? 1 : -1;
            return 0;
        });

        rowsArray.forEach(row => tbody.appendChild(row));
    }

    // Initial setup
    applyAllFilters();
}
