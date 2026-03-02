const state = {
    pendingRequests: 0,
    dbTables: [],
    activeTableName: ""
};

const elements = {
    apiStatus: document.getElementById("api-status"),
    totalDevices: document.getElementById("metric-devices"),
    totalUsers: document.getElementById("metric-users"),
    lastOperation: document.getElementById("metric-operation"),
    lastOperationDetail: document.getElementById("metric-operation-detail"),
    responseSummary: document.getElementById("response-summary"),
    responseOutput: document.getElementById("response-output"),
    devicesTableBody: document.getElementById("devices-table-body"),
    refreshDashboard: document.getElementById("refresh-dashboard"),
    deviceTypeOptions: document.getElementById("device-type-options"),
    dbTableSelect: document.getElementById("db-table-select"),
    dbTableSummary: document.getElementById("db-table-summary"),
    dbLoadAll: document.getElementById("db-load-all"),
    dbResultsHead: document.getElementById("db-results-head"),
    dbResultsBody: document.getElementById("db-results-body"),
    dbGetByKeyForm: document.getElementById("db-get-by-key-form"),
    dbFilterForm: document.getElementById("db-filter-form"),
    dbFilterColumn: document.getElementById("db-filter-column"),
    dbKeyLabel: document.getElementById("db-key-label"),
    dbKeyValue: document.getElementById("db-key-value")
};

function setStatus(text, variant) {
    elements.apiStatus.textContent = text;
    elements.apiStatus.className = `pill ${variant}`;
}

function updatePending(delta) {
    state.pendingRequests += delta;
    if (state.pendingRequests > 0) {
        setStatus("Working", "pill-waiting");
        return;
    }

    state.pendingRequests = 0;
}

function formatUtc(value) {
    if (!value) {
        return "-";
    }

    const parsed = new Date(value);
    return Number.isNaN(parsed.getTime()) ? value : parsed.toLocaleString();
}

function formatValue(value) {
    if (value == null) {
        return "-";
    }

    if (typeof value === "string" && value.includes("T")) {
        const parsed = new Date(value);
        if (!Number.isNaN(parsed.getTime())) {
            return parsed.toLocaleString();
        }
    }

    return String(value);
}

function setLastOperation(name, detail) {
    elements.lastOperation.textContent = name;
    elements.lastOperationDetail.textContent = detail;
}

function renderResponse(operation, status, payload) {
    elements.responseSummary.textContent = `${operation} returned HTTP ${status}.`;
    elements.responseOutput.textContent = JSON.stringify(payload, null, 2);
}

function clearChildren(element) {
    element.replaceChildren();
}

function initializeTabs() {
    const buttons = Array.from(document.querySelectorAll(".tab-button"));
    const panels = Array.from(document.querySelectorAll(".tab-panel"));

    function activate(targetId) {
        for (const button of buttons) {
            const isActive = button.dataset.tabTarget === targetId;
            button.classList.toggle("is-active", isActive);
            button.setAttribute("aria-selected", isActive ? "true" : "false");
        }

        for (const panel of panels) {
            const isActive = panel.id === targetId;
            panel.classList.toggle("is-active", isActive);
            panel.hidden = !isActive;
        }
    }

    for (const button of buttons) {
        button.addEventListener("click", () => activate(button.dataset.tabTarget));
    }
}

function renderTable(headElement, bodyElement, columns, rows, emptyMessage) {
    clearChildren(headElement);
    clearChildren(bodyElement);

    const normalizedColumns = Array.isArray(columns) ? columns : [];
    if (normalizedColumns.length === 0) {
        const row = document.createElement("tr");
        const cell = document.createElement("td");
        cell.className = "empty-state";
        cell.textContent = emptyMessage;
        row.appendChild(cell);
        bodyElement.appendChild(row);
        return;
    }

    const headerRow = document.createElement("tr");
    for (const column of normalizedColumns) {
        const th = document.createElement("th");
        th.textContent = typeof column === "string" ? column : column.name;
        headerRow.appendChild(th);
    }

    headElement.appendChild(headerRow);

    if (!Array.isArray(rows) || rows.length === 0) {
        const row = document.createElement("tr");
        const cell = document.createElement("td");
        cell.colSpan = normalizedColumns.length;
        cell.className = "empty-state";
        cell.textContent = emptyMessage;
        row.appendChild(cell);
        bodyElement.appendChild(row);
        return;
    }

    for (const sourceRow of rows) {
        const row = document.createElement("tr");
        for (const column of normalizedColumns) {
            const columnName = typeof column === "string" ? column : column.name;
            const cell = document.createElement("td");
            cell.textContent = formatValue(sourceRow?.[columnName]);
            row.appendChild(cell);
        }

        bodyElement.appendChild(row);
    }
}

function renderDevices(devices) {
    const rows = Array.isArray(devices)
        ? devices.map((device) => ({
            id: device.id ?? "-",
            publicDeviceId: device.publicDeviceId ?? "-",
            name: device.name ?? "-",
            createdAtUtc: formatUtc(device.createdAtUtc)
        }))
        : [];

    renderTable(
        document.createElement("thead"),
        elements.devicesTableBody,
        ["id", "publicDeviceId", "name", "createdAtUtc"],
        rows,
        "No devices returned."
    );
}

async function callApi(operation, path, options = {}) {
    updatePending(1);
    setLastOperation(operation, "Request in flight.");

    try {
        const response = await fetch(path, {
            headers: {
                "Content-Type": "application/json",
                ...(options.headers ?? {})
            },
            ...options
        });

        let payload = null;
        const contentType = response.headers.get("content-type") ?? "";
        if (contentType.includes("application/json")) {
            payload = await response.json();
        } else {
            const text = await response.text();
            payload = text ? { raw: text } : {};
        }

        const detail = response.ok
            ? "Completed successfully."
            : payload?.error?.message ?? "Request failed.";

        setLastOperation(operation, detail);
        renderResponse(operation, response.status, payload);
        setStatus(response.ok ? "Healthy" : "Error", response.ok ? "pill-success" : "pill-error");

        return { response, payload };
    } catch (error) {
        const message = error instanceof Error ? error.message : String(error);
        const payload = { error: { message } };
        setLastOperation(operation, message);
        renderResponse(operation, "network", payload);
        setStatus("Offline", "pill-error");
        return { response: null, payload };
    } finally {
        updatePending(-1);
        if (state.pendingRequests === 0 && elements.apiStatus.textContent === "Working") {
            setStatus("Ready", "pill-waiting");
        }
    }
}

async function loadTotalDevices() {
    const { payload, response } = await callApi("Load total devices", "/api/devices/total");
    if (response?.ok) {
        elements.totalDevices.textContent = payload?.value ?? "-";
    }
}

async function loadTotalUsers() {
    const { payload, response } = await callApi("Load total users", "/api/users/total");
    if (response?.ok) {
        elements.totalUsers.textContent = payload?.value ?? "-";
    }
}

async function listDevices() {
    const { payload, response } = await callApi("List devices", "/api/devices");
    if (response?.ok) {
        renderDevices(payload?.value ?? []);
    }
}

function getFormValues(form) {
    return Object.fromEntries(new FormData(form).entries());
}

function getActiveTable() {
    return state.dbTables.find((table) => table.name === state.activeTableName) ?? null;
}

function renderExplorerMetadata() {
    const table = getActiveTable();
    if (!table) {
        elements.dbTableSummary.textContent = "No tables available.";
        elements.dbKeyLabel.textContent = "Primary key";
        elements.dbKeyValue.disabled = true;
        clearChildren(elements.dbFilterColumn);
        return;
    }

    elements.dbTableSummary.textContent = `${table.name} | ${table.columnCount} columns | primary key: ${table.primaryKeyColumn ?? "none"}`;
    elements.dbKeyLabel.textContent = table.primaryKeyColumn ?? "Primary key not available";
    elements.dbKeyValue.disabled = !table.primaryKeyColumn;

    clearChildren(elements.dbFilterColumn);
    for (const column of table.columns) {
        const option = document.createElement("option");
        option.value = column.name;
        option.textContent = column.name;
        elements.dbFilterColumn.appendChild(option);
    }
}

function renderDbTableOptions() {
    clearChildren(elements.dbTableSelect);

    for (const table of state.dbTables) {
        const option = document.createElement("option");
        option.value = table.name;
        option.textContent = table.name;
        elements.dbTableSelect.appendChild(option);
    }

    if (!state.activeTableName && state.dbTables.length > 0) {
        state.activeTableName = state.dbTables[0].name;
    }

    elements.dbTableSelect.value = state.activeTableName;
    renderExplorerMetadata();
}

function inferDeviceTypeLabel(row, primaryKey) {
    const preferred = ["name", "display_name", "label", "title", "description", "type_name"];
    for (const key of preferred) {
        if (row[key] != null) {
            return `${row[primaryKey]} - ${row[key]}`;
        }
    }

    const otherKey = Object.keys(row).find((key) => key !== primaryKey && row[key] != null);
    return otherKey ? `${row[primaryKey]} - ${row[otherKey]}` : String(row[primaryKey]);
}

function renderDeviceTypeOptions(rowsDto) {
    const rows = rowsDto?.rows ?? [];
    const columns = rowsDto?.columns ?? [];
    const primaryKey = rowsDto?.primaryKeyColumn ?? columns[0]?.name;
    if (!primaryKey || rows.length === 0) {
        return;
    }

    clearChildren(elements.deviceTypeOptions);
    for (const row of rows) {
        const keyValue = row?.[primaryKey];
        if (keyValue == null) {
            continue;
        }

        const label = document.createElement("label");
        label.className = "option-card";

        const input = document.createElement("input");
        input.name = "deviceTypeId";
        input.type = "radio";
        input.value = String(keyValue);
        input.required = true;

        const text = document.createElement("span");
        text.textContent = inferDeviceTypeLabel(row, primaryKey);

        label.appendChild(input);
        label.appendChild(text);
        elements.deviceTypeOptions.appendChild(label);
    }
}

async function loadDbTables() {
    const { payload, response } = await callApi("Load database tables", "/api/db/tables");
    if (!response?.ok) {
        return;
    }

    state.dbTables = payload?.value ?? [];
    if (!state.activeTableName && state.dbTables.length > 0) {
        state.activeTableName = state.dbTables[0].name;
    }

    renderDbTableOptions();
}

async function loadExplorerRows() {
    const table = getActiveTable();
    if (!table) {
        return;
    }

    const { payload, response } = await callApi("Get all rows", `/api/db/tables/${encodeURIComponent(table.name)}/rows?limit=50&offset=0`);
    if (response?.ok) {
        renderTable(elements.dbResultsHead, elements.dbResultsBody, payload?.value?.columns ?? [], payload?.value?.rows ?? [], "No records returned.");
    }
}

async function loadDeviceTypeRows() {
    const target = state.dbTables.find((table) => table.name.toLowerCase().includes("device_type"));
    if (!target) {
        return;
    }

    const { payload, response } = await callApi("Load device type options", `/api/db/tables/${encodeURIComponent(target.name)}/rows?limit=50&offset=0`);
    if (response?.ok) {
        renderDeviceTypeOptions(payload?.value);
    }
}

async function handleGetDevice(event) {
    event.preventDefault();
    const values = getFormValues(event.currentTarget);
    await callApi("Get device by ID", `/api/devices/${values.deviceId}`);
}

async function handleCreateDevice(event) {
    event.preventDefault();
    const values = getFormValues(event.currentTarget);
    const payload = {
        publicDeviceId: values.publicDeviceId,
        name: values.name,
        deviceTypeId: Number(values.deviceTypeId),
        ownerUserId: Number(values.ownerUserId)
    };

    const result = await callApi("Create device", "/api/devices", {
        method: "POST",
        body: JSON.stringify(payload)
    });

    if (result.response?.ok) {
        event.currentTarget.reset();
        await Promise.all([listDevices(), loadTotalDevices()]);
    }
}

async function handleUpdateDevice(event) {
    event.preventDefault();
    const values = getFormValues(event.currentTarget);
    const payload = {
        publicDeviceId: values.publicDeviceId,
        name: values.name
    };

    const result = await callApi(
        "Update device",
        `/api/devices/by-public-id/${encodeURIComponent(values.targetPublicDeviceId)}`,
        {
            method: "PUT",
            body: JSON.stringify(payload)
        });

    if (result.response?.ok) {
        event.currentTarget.reset();
        await listDevices();
    }
}

async function handleDeleteDevice(event) {
    event.preventDefault();
    const values = getFormValues(event.currentTarget);
    const result = await callApi("Delete device", `/api/devices/${values.deviceId}`, {
        method: "DELETE"
    });

    if (result.response?.ok) {
        event.currentTarget.reset();
        await Promise.all([listDevices(), loadTotalDevices()]);
    }
}

async function handleLastDevice(event) {
    event.preventDefault();
    const values = getFormValues(event.currentTarget);
    await callApi("Last used device by user", `/api/users/${values.userId}/last-device`);
}

async function handleUserDates(event) {
    event.preventDefault();
    const values = getFormValues(event.currentTarget);
    await callApi("User created and updated dates", `/api/users/${values.userId}/dates`);
}

async function handleMarkInactive(event) {
    event.preventDefault();
    const values = getFormValues(event.currentTarget);
    await callApi("Mark user inactive if stale", `/api/users/${values.userId}/mark-inactive-if-stale`, {
        method: "POST"
    });
}

async function handleDbGetByKey(event) {
    event.preventDefault();
    const table = getActiveTable();
    if (!table?.primaryKeyColumn) {
        return;
    }

    const values = getFormValues(event.currentTarget);
    const { payload, response } = await callApi(
        "Get single record",
        `/api/db/tables/${encodeURIComponent(table.name)}/rows/by-key/${encodeURIComponent(values.keyValue)}`
    );

    if (response?.ok) {
        const row = payload?.value?.values ? [payload.value.values] : [];
        renderTable(elements.dbResultsHead, elements.dbResultsBody, payload?.value?.columns ?? [], row, "No record returned.");
    }
}

async function handleDbFilter(event) {
    event.preventDefault();
    const table = getActiveTable();
    if (!table) {
        return;
    }

    const values = getFormValues(event.currentTarget);
    const search = new URLSearchParams({
        value: values.value,
        limit: values.limit
    });

    const { payload, response } = await callApi(
        "Get subset of records",
        `/api/db/tables/${encodeURIComponent(table.name)}/rows/by-column/${encodeURIComponent(values.columnName)}?${search.toString()}`
    );

    if (response?.ok) {
        renderTable(elements.dbResultsHead, elements.dbResultsBody, payload?.value?.columns ?? [], payload?.value?.rows ?? [], "No matching records.");
    }
}

function handleDbTableChange(event) {
    state.activeTableName = event.currentTarget.value;
    renderExplorerMetadata();
    loadExplorerRows();
}

async function refreshDashboard() {
    await Promise.all([listDevices(), loadTotalDevices(), loadTotalUsers(), loadDbTables()]);
    await Promise.all([loadExplorerRows(), loadDeviceTypeRows()]);
}

document.querySelector('[data-action="load-total-devices"]').addEventListener("click", loadTotalDevices);
document.querySelector('[data-action="load-total-users"]').addEventListener("click", loadTotalUsers);
document.querySelector('[data-action="list-devices"]').addEventListener("click", listDevices);
elements.refreshDashboard.addEventListener("click", refreshDashboard);
elements.dbLoadAll.addEventListener("click", loadExplorerRows);
elements.dbTableSelect.addEventListener("change", handleDbTableChange);

document.getElementById("get-device-form").addEventListener("submit", handleGetDevice);
document.getElementById("create-device-form").addEventListener("submit", handleCreateDevice);
document.getElementById("update-device-form").addEventListener("submit", handleUpdateDevice);
document.getElementById("delete-device-form").addEventListener("submit", handleDeleteDevice);
document.getElementById("last-device-form").addEventListener("submit", handleLastDevice);
document.getElementById("user-dates-form").addEventListener("submit", handleUserDates);
document.getElementById("mark-inactive-form").addEventListener("submit", handleMarkInactive);
elements.dbGetByKeyForm.addEventListener("submit", handleDbGetByKey);
elements.dbFilterForm.addEventListener("submit", handleDbFilter);

setStatus("Ready", "pill-waiting");
initializeTabs();
refreshDashboard();
