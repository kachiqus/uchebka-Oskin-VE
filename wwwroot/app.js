(() => {
  "use strict";

  const $ = (id) => document.getElementById(id);
  const page = document.body.dataset.page || "app";

  const shared = {
    msgModal: $("msg_modal"),
    msgKind: $("msg_kind"),
    msgTitle: $("msg_title"),
    msgBody: $("msg_body"),
    msgDetails: $("msg_details"),
    loginUser: $("login_user"),
    loginPass: $("login_pass"),
    btnDoLogin: $("btn_do_login"),
    btnRegister: $("btn_register"),
    btnGoApp: $("btn_go_app"),
    errLogin: $("err_login"),
    errPass: $("err_pass"),
  };

  const app = {
    authStatus: $("auth_status"),
    selectedId: $("selected_id"),
    searchCount: $("search_count"),
    selectedTotalTime: $("selected_total_time"),
    searchQ: $("search_q"),
    cardsGrid: $("cards_grid"),
    detailsTitleInline: $("details_title_inline"),
    detailsBody: $("details_body"),
    btnDetailsCopy: $("btn_details_copy"),
    workshopsList: $("workshops_list"),
    workshopTotalHours: $("workshop_total_hours"),
    workshopCount: $("workshop_count"),
    btnRefreshWorkshops: $("btn_refresh_workshops"),
    btnAddWorkshop: $("btn_add_workshop"),
    btnReload: $("btn_reload"),
    btnOpenCreate: $("btn_open_create"),
    btnOpenEdit: $("btn_open_edit"),
    btnOpenView: $("btn_open_view"),
    btnOpenDelete: $("btn_open_delete"),
    btnCalcMaterial: $("btn_calc_material"),
    calcProductType: $("calc_product_type"),
    calcMaterialType: $("calc_material_type"),
    calcCount: $("calc_count"),
    calcParam1: $("calc_param1"),
    calcParam2: $("calc_param2"),
    calcResult: $("calc_result"),
    navProducts: $("nav_products"),
    navLogs: $("nav_logs"),
    navExit: $("nav_exit"),
    viewLogs: $("view_logs"),
    btnLogsClear: $("btn_logs_clear"),
    log: $("log"),
    btnTokenToggle: $("btn_token_toggle"),
    tokenModal: $("token_modal"),
    tokenBox: $("token_box"),
    btnTokenCopy: $("btn_token_copy"),
    btnLoginPage: $("btn_login_page"),
    btnLogout: $("btn_logout"),
    formModal: $("form_modal"),
    formBadge: $("form_badge"),
    formTitle: $("form_title"),
    formFields: $("form_fields"),
    btnFormSubmit: $("btn_form_submit"),
    workshopModal: $("workshop_modal"),
    workshopFormBadge: $("workshop_form_badge"),
    workshopFormTitle: $("workshop_form_title"),
    workshopFormFields: $("workshop_form_fields"),
    btnWorkshopSubmit: $("btn_workshop_submit"),
    confirmModal: $("confirm_modal"),
    confirmTitle: $("confirm_title"),
    confirmBody: $("confirm_body"),
    btnConfirmYes: $("btn_confirm_yes"),
  };

  const state = {
    token: localStorage.getItem("pggen_token") || "",
    apiBase: "/api",
    products: [],
    filteredProducts: [],
    productTypes: [],
    materialTypes: [],
    workshops: [],
    selectedProduct: null,
    selectedWorkshops: [],
    confirmResolve: null,
    workshopResolve: null,
    formMode: "create",
    workshopFormMode: "create",
    editingWorkshop: null,
    isBusy: false,
  };

  function openModal(el){ if (el) el.classList.add("open"); }
  function closeModal(el){ if (el) el.classList.remove("open"); }

  function wireModalClose(modalEl, onClose) {
    if (!modalEl) return;
    modalEl.addEventListener("click", (ev) => {
      const target = ev.target;
      if (target?.getAttribute?.("data-close") === "1") {
        closeModal(modalEl);
        if (onClose) onClose();
      }
    });
  }

  function setBadge(el, kind) {
    if (!el) return;
    el.classList.remove("ok","warn","err");
    if (kind) el.classList.add(kind);
  }

  function showMessage(kind, title, body, details = "") {
    if (!shared.msgModal) {
      alert(`${title}\n\n${body}`);
      return;
    }
    shared.msgKind.textContent = kind === "err" ? "ОШИБКА" : kind === "warn" ? "ВНИМАНИЕ" : "ИНФО";
    setBadge(shared.msgKind, kind);
    shared.msgTitle.textContent = title || "Сообщение";
    shared.msgBody.textContent = body || "";
    if (shared.msgDetails) shared.msgDetails.textContent = details || "";
    openModal(shared.msgModal);
  }

  function confirmDialog(title, body) {
    return new Promise((resolve) => {
      state.confirmResolve = resolve;
      app.confirmTitle.textContent = title;
      app.confirmBody.textContent = body;
      openModal(app.confirmModal);
    });
  }

  function log(line) {
    if (!app.log) return;
    const stamp = new Date().toLocaleString("ru-RU");
    app.log.textContent = `[${stamp}] ${line}\n` + app.log.textContent;
  }

  function setBusy(isBusy) {
    state.isBusy = !!isBusy;
    const controls = document.querySelectorAll("button, input, select");
    controls.forEach((el) => {
      if (el.id === "search_q" || el.id === "calc_count" || el.id === "calc_param1" || el.id === "calc_param2" || el.id === "calc_product_type" || el.id === "calc_material_type") return;
      el.disabled = state.isBusy;
    });
  }

  function apiUrl(path) {
    const normalized = path.startsWith("/") ? path : `/${path}`;
    return `${state.apiBase}${normalized}`;
  }

  async function http(method, url, body, expectJson = true) {
    const headers = {};
    if (body !== undefined) headers["Content-Type"] = "application/json";
    if (state.token) headers.Authorization = `Bearer ${state.token}`;

    const res = await fetch(url, {
      method,
      headers,
      body: body !== undefined ? JSON.stringify(body) : undefined,
    });

    const text = await res.text();
    let data = null;
    if (text) {
      try { data = JSON.parse(text); } catch { data = text; }
    }

    if (!res.ok) {
      const details = typeof data === "string" ? data : JSON.stringify(data, null, 2);
      const message = typeof data === "object" && data?.title ? `${res.status} ${data.title}` : `${res.status} ${res.statusText}`;
      const err = new Error(message);
      err.details = details;
      throw err;
    }

    return expectJson ? data : text;
  }

  function goToLogin() { window.location.href = "login.html"; }
  function goToApp() { window.location.href = "index.html"; }

  async function doLogin(isRegister) {
    const username = (shared.loginUser?.value || "").trim();
    const password = (shared.loginPass?.value || "").trim();

    if (shared.errLogin) shared.errLogin.textContent = username ? "" : "Введите логин.";
    if (shared.errPass) shared.errPass.textContent = password ? "" : "Введите пароль.";
    if (!username || !password) return;

    try {
      setBusy(true);
      const result = await http("POST", apiUrl(isRegister ? "/Auth/register" : "/Auth/login"), { username, password }, true);
      const token = result?.token || result?.accessToken || result?.jwt || "";
      if (!token) throw new Error("Сервер не вернул токен доступа.");
      state.token = token;
      localStorage.setItem("pggen_token", token);
      goToApp();
    } catch (err) {
      showMessage("err", isRegister ? "Ошибка регистрации" : "Ошибка входа", err.message, err.details || "");
    } finally {
      setBusy(false);
    }
  }

  function doLogout() {
    localStorage.removeItem("pggen_token");
    state.token = "";
    goToLogin();
  }

  function setAuthUI() {
    if (app.authStatus) app.authStatus.textContent = state.token ? "Авторизован" : "Не авторизован";
    if (app.tokenBox) app.tokenBox.textContent = state.token || "Токен отсутствует";
  }

  function formatPrice(value) {
    return new Intl.NumberFormat("ru-RU", { style: "currency", currency: "RUB", maximumFractionDigits: 2 }).format(Number(value || 0));
  }

  function escapeHtml(value) {
    return String(value ?? "")
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;")
      .replace(/'/g, "&#39;");
  }

  function populateSelect(selectEl, rows, valueKey, labelKey, placeholder) {
    if (!selectEl) return;
    selectEl.innerHTML = `<option value="">${placeholder}</option>`;
    rows.forEach((row) => {
      const option = document.createElement("option");
      option.value = String(row[valueKey]);
      option.textContent = row[labelKey];
      selectEl.appendChild(option);
    });
  }

  async function loadLookups() {
    const [productTypes, materialTypes, workshops] = await Promise.all([
      http("GET", apiUrl("/ProductType"), undefined, true),
      http("GET", apiUrl("/MaterialType"), undefined, true),
      http("GET", apiUrl("/Workshop"), undefined, true),
    ]);
    state.productTypes = Array.isArray(productTypes) ? productTypes : [];
    state.materialTypes = Array.isArray(materialTypes) ? materialTypes : [];
    state.workshops = Array.isArray(workshops) ? workshops : [];
    populateSelect(app.calcProductType, state.productTypes, "productTypeId", "name", "Выберите тип продукции");
    populateSelect(app.calcMaterialType, state.materialTypes, "materialTypeId", "name", "Выберите тип материала");
  }

  async function loadProducts() {
    const rows = await http("GET", apiUrl("/Product"), undefined, true);
    state.products = Array.isArray(rows) ? rows : [];
    filterProducts();
    if (state.selectedProduct) {
      const fresh = state.products.find((x) => x.productId === state.selectedProduct.productId || x.ProductId === state.selectedProduct.ProductId);
      if (fresh) selectProduct(fresh.productId || fresh.ProductId, false);
    }
  }

  function normalizeProduct(row) {
    return {
      productId: row.productId ?? row.ProductId,
      article: row.article ?? row.Article,
      name: row.name ?? row.Name,
      productTypeId: row.productTypeId ?? row.ProductTypeId,
      productTypeName: row.productTypeName ?? row.ProductTypeName,
      minPartnerPrice: row.minPartnerPrice ?? row.MinPartnerPrice,
      mainMaterialTypeId: row.mainMaterialTypeId ?? row.MainMaterialTypeId,
      mainMaterialTypeName: row.mainMaterialTypeName ?? row.MainMaterialTypeName,
      manufactureTimeHours: row.manufactureTimeHours ?? row.ManufactureTimeHours ?? 0,
    };
  }

  function filterProducts() {
    const query = (app.searchQ?.value || "").trim().toLowerCase();
    state.filteredProducts = state.products.map(normalizeProduct).filter((row) => {
      if (!query) return true;
      return [row.article, row.name, row.productTypeName, row.mainMaterialTypeName].some((value) => String(value ?? "").toLowerCase().includes(query));
    });
    renderProducts();
  }

  function renderProducts() {
    if (!app.cardsGrid) return;
    app.cardsGrid.innerHTML = "";

    if (!state.filteredProducts.length) {
      app.cardsGrid.innerHTML = '<div class="empty-state">По вашему запросу продукция не найдена.</div>';
      if (app.searchCount) app.searchCount.textContent = "0";
      return;
    }

    state.filteredProducts.forEach((product) => {
      const card = document.createElement("article");
      card.className = "carditem" + (state.selectedProduct?.productId === product.productId ? " selected" : "");
      card.dataset.id = String(product.productId);
      card.innerHTML = `
        <div class="card-top">
          <div>
            <div class="card-badge">Арт. ${escapeHtml(product.article)}</div>
            <h4>${escapeHtml(product.name)}</h4>
          </div>
          <span class="badge">${escapeHtml(product.productTypeName || "Тип не указан")}</span>
        </div>
        <div class="card-meta">
          <div class="meta-row"><span class="meta-label">Материал</span><strong>${escapeHtml(product.mainMaterialTypeName || "—")}</strong></div>
          <div class="meta-row"><span class="meta-label">Мин. цена</span><strong>${escapeHtml(formatPrice(product.minPartnerPrice))}</strong></div>
          <div class="meta-row"><span class="meta-label">Время изготовления</span><strong>${escapeHtml(product.manufactureTimeHours)} ч</strong></div>
        </div>`;
      card.addEventListener("click", () => selectProduct(product.productId));
      card.addEventListener("dblclick", () => openProductForm("edit"));
      app.cardsGrid.appendChild(card);
    });

    if (app.searchCount) app.searchCount.textContent = String(state.filteredProducts.length);
  }

  async function selectProduct(productId, reloadWorkshops = true) {
    const product = state.filteredProducts.find((x) => x.productId === productId) || state.products.map(normalizeProduct).find((x) => x.productId === productId);
    if (!product) return;
    state.selectedProduct = product;
    if (app.selectedId) app.selectedId.textContent = String(product.productId);
    if (app.selectedTotalTime) app.selectedTotalTime.textContent = String(product.manufactureTimeHours || 0);
    renderProducts();
    renderSelectedProduct();
    if (reloadWorkshops) await loadSelectedProductWorkshops();
    if (app.calcProductType) app.calcProductType.value = String(product.productTypeId);
    if (app.calcMaterialType) app.calcMaterialType.value = String(product.mainMaterialTypeId);
  }

  function renderSelectedProduct() {
    const product = state.selectedProduct;
    if (!product) {
      if (app.detailsTitleInline) app.detailsTitleInline.textContent = "Продукция не выбрана";
      if (app.detailsBody) app.detailsBody.className = "details-body empty-state";
      if (app.detailsBody) app.detailsBody.textContent = "Выберите изделие из списка слева.";
      return;
    }

    app.detailsTitleInline.textContent = product.name;
    app.detailsBody.className = "details-body";
    app.detailsBody.innerHTML = `
      <div class="details-grid">
        <div class="details-grid__row"><span class="meta-label">Идентификатор</span><strong>${escapeHtml(product.productId)}</strong></div>
        <div class="details-grid__row"><span class="meta-label">Артикул</span><strong>${escapeHtml(product.article)}</strong></div>
        <div class="details-grid__row"><span class="meta-label">Тип продукции</span><strong>${escapeHtml(product.productTypeName || "—")}</strong></div>
        <div class="details-grid__row"><span class="meta-label">Основной материал</span><strong>${escapeHtml(product.mainMaterialTypeName || "—")}</strong></div>
        <div class="details-grid__row"><span class="meta-label">Минимальная стоимость</span><strong>${escapeHtml(formatPrice(product.minPartnerPrice))}</strong></div>
        <div class="details-grid__row"><span class="meta-label">Время изготовления</span><strong>${escapeHtml(product.manufactureTimeHours)} ч</strong></div>
      </div>`;
  }

  async function loadSelectedProductWorkshops() {
    if (!state.selectedProduct) {
      state.selectedWorkshops = [];
      renderWorkshops();
      return;
    }

    const rows = await http("GET", apiUrl(`/Product/${state.selectedProduct.productId}/workshops`), undefined, true);
    state.selectedWorkshops = (Array.isArray(rows) ? rows : []).map((row) => ({
      productId: row.productId ?? row.ProductId,
      workshopId: row.workshopId ?? row.WorkshopId,
      workshopName: row.workshopName ?? row.WorkshopName,
      peopleCount: row.peopleCount ?? row.PeopleCount,
      manufactureHours: row.manufactureHours ?? row.ManufactureHours,
    }));
    const total = state.selectedWorkshops.reduce((sum, item) => sum + Number(item.manufactureHours || 0), 0);
    state.selectedProduct.manufactureTimeHours = Math.ceil(total);
    if (app.selectedTotalTime) app.selectedTotalTime.textContent = String(state.selectedProduct.manufactureTimeHours);
    renderSelectedProduct();
    renderWorkshops();
  }

  function renderWorkshops() {
    if (!app.workshopsList) return;
    if (!state.selectedProduct) {
      app.workshopsList.className = "workshops-list empty-state";
      app.workshopsList.textContent = "Выберите продукцию, чтобы увидеть список цехов.";
      if (app.workshopTotalHours) app.workshopTotalHours.textContent = "0";
      if (app.workshopCount) app.workshopCount.textContent = "0";
      return;
    }

    if (!state.selectedWorkshops.length) {
      app.workshopsList.className = "workshops-list empty-state";
      app.workshopsList.textContent = "Для выбранной продукции пока не указаны цеха.";
      if (app.workshopTotalHours) app.workshopTotalHours.textContent = "0";
      if (app.workshopCount) app.workshopCount.textContent = "0";
      return;
    }

    app.workshopsList.className = "workshops-list";
    app.workshopsList.innerHTML = "";
    let total = 0;
    state.selectedWorkshops.forEach((item) => {
      total += Number(item.manufactureHours || 0);
      const card = document.createElement("article");
      card.className = "workshop-card";
      card.innerHTML = `
        <div class="workshop-card__top">
          <div>
            <div class="card-badge">Цех #${escapeHtml(item.workshopId)}</div>
            <h4>${escapeHtml(item.workshopName || "Без названия")}</h4>
          </div>
          <div class="workshop-card__actions">
            <button class="btn btn-lite" data-action="edit">Изменить</button>
            <button class="btn btn-danger" data-action="delete">Удалить</button>
          </div>
        </div>
        <div class="meta-row"><span class="meta-label">Количество человек</span><strong>${escapeHtml(item.peopleCount)}</strong></div>
        <div class="meta-row"><span class="meta-label">Время в цехе</span><strong>${escapeHtml(item.manufactureHours)} ч</strong></div>`;
      card.querySelector('[data-action="edit"]').addEventListener("click", () => openWorkshopForm("edit", item));
      card.querySelector('[data-action="delete"]').addEventListener("click", () => deleteWorkshop(item));
      app.workshopsList.appendChild(card);
    });

    if (app.workshopTotalHours) app.workshopTotalHours.textContent = String(Math.ceil(total));
    if (app.workshopCount) app.workshopCount.textContent = String(state.selectedWorkshops.length);
  }

  function getProductTypeOptions(selectedValue) {
    return state.productTypes.map((row) => {
      const id = row.productTypeId ?? row.ProductTypeId;
      const name = row.name ?? row.Name;
      return `<option value="${id}" ${String(selectedValue) === String(id) ? "selected" : ""}>${escapeHtml(name)}</option>`;
    }).join("");
  }

  function getMaterialTypeOptions(selectedValue) {
    return state.materialTypes.map((row) => {
      const id = row.materialTypeId ?? row.MaterialTypeId;
      const name = row.name ?? row.Name;
      return `<option value="${id}" ${String(selectedValue) === String(id) ? "selected" : ""}>${escapeHtml(name)}</option>`;
    }).join("");
  }

  function getWorkshopOptions(selectedValue, disableSelected) {
    return state.workshops.map((row) => {
      const id = row.workshopId ?? row.WorkshopId;
      const name = row.name ?? row.Name;
      const disabled = disableSelected ? "disabled" : "";
      return `<option value="${id}" ${String(selectedValue) === String(id) ? "selected" : ""} ${disabled}>${escapeHtml(name)}</option>`;
    }).join("");
  }

  function openProductForm(mode) {
    state.formMode = mode;
    const product = mode === "edit" ? state.selectedProduct : null;
    if (mode === "edit" && !product) {
      showMessage("warn", "Нет выбранной продукции", "Сначала выберите карточку продукции.");
      return;
    }
    app.formBadge.textContent = mode === "create" ? "ДОБАВЛЕНИЕ" : "РЕДАКТИРОВАНИЕ";
    setBadge(app.formBadge, mode === "create" ? "ok" : "warn");
    app.formTitle.textContent = mode === "create" ? "Добавление продукции" : "Редактирование продукции";
    app.formFields.innerHTML = `
      <div class="field field--full">
        <label for="product_article">Артикул</label>
        <input id="product_article" type="number" min="1" step="1" value="${product ? escapeHtml(product.article) : ""}" />
        <div class="errtxt" id="err_article"></div>
      </div>
      <div class="field field--full">
        <label for="product_type_id">Тип продукции</label>
        <select id="product_type_id"><option value="">Выберите тип продукции</option>${getProductTypeOptions(product?.productTypeId)}</select>
        <div class="errtxt" id="err_product_type"></div>
      </div>
      <div class="field field--full">
        <label for="product_name">Наименование</label>
        <input id="product_name" value="${product ? escapeHtml(product.name) : ""}" />
        <div class="errtxt" id="err_name"></div>
      </div>
      <div class="field field--full">
        <label for="product_price">Минимальная стоимость для партнера</label>
        <input id="product_price" type="number" min="0" step="0.01" value="${product ? escapeHtml(product.minPartnerPrice) : ""}" />
        <div class="errtxt" id="err_price"></div>
      </div>
      <div class="field field--full">
        <label for="product_material_id">Основной материал</label>
        <select id="product_material_id"><option value="">Выберите основной материал</option>${getMaterialTypeOptions(product?.mainMaterialTypeId)}</select>
        <div class="errtxt" id="err_material"></div>
      </div>`;
    openModal(app.formModal);
  }

  function validateProductForm() {
    const article = $("product_article")?.value.trim() || "";
    const productTypeId = $("product_type_id")?.value || "";
    const name = $("product_name")?.value.trim() || "";
    const price = $("product_price")?.value.trim() || "";
    const materialTypeId = $("product_material_id")?.value || "";

    $("err_article").textContent = article ? "" : "Укажите артикул.";
    $("err_product_type").textContent = productTypeId ? "" : "Выберите тип продукции.";
    $("err_name").textContent = name ? "" : "Введите наименование.";
    $("err_price").textContent = price ? "" : "Укажите стоимость.";
    $("err_material").textContent = materialTypeId ? "" : "Выберите основной материал.";

    if (article && !Number.isInteger(Number(article))) $("err_article").textContent = "Артикул должен быть целым числом.";
    if (price && Number(price) < 0) $("err_price").textContent = "Стоимость не может быть отрицательной.";

    const hasErrors = ["err_article","err_product_type","err_name","err_price","err_material"].some((id) => $(id).textContent);
    if (hasErrors) return null;

    return {
      Article: Number(article),
      ProductTypeId: Number(productTypeId),
      Name: name,
      MinPartnerPrice: Number(price),
      MainMaterialTypeId: Number(materialTypeId),
    };
  }

  async function submitProductForm() {
    const payload = validateProductForm();
    if (!payload) {
      showMessage("warn", "Проверьте форму", "Исправьте ошибки в карточке продукции.");
      return;
    }

    try {
      setBusy(true);
      if (state.formMode === "create") {
        await http("POST", apiUrl("/Product"), payload, true);
        log("Добавлена новая продукция.");
      } else {
        const patch = Object.entries(payload).map(([key, value]) => ({ op: "replace", path: `/${key}`, value }));
        await http("PATCH", apiUrl(`/Product/${state.selectedProduct.productId}`), patch, true);
        log(`Обновлена продукция ID ${state.selectedProduct.productId}.`);
      }
      closeModal(app.formModal);
      await loadProducts();
      if (state.selectedProduct) await selectProduct(state.selectedProduct.productId);
      showMessage("ok", "Готово", "Карточка продукции сохранена.");
    } catch (err) {
      showMessage("err", "Ошибка сохранения", err.message, err.details || "");
    } finally {
      setBusy(false);
    }
  }

  async function deleteSelectedProduct() {
    if (!state.selectedProduct) {
      showMessage("warn", "Нет выбранной продукции", "Сначала выберите карточку продукции.");
      return;
    }
    const ok = await confirmDialog("Удаление продукции", `Удалить продукцию «${state.selectedProduct.name}»?`);
    if (!ok) return;
    try {
      setBusy(true);
      await http("DELETE", apiUrl(`/Product/${state.selectedProduct.productId}`), undefined, false);
      log(`Удалена продукция ID ${state.selectedProduct.productId}.`);
      state.selectedProduct = null;
      state.selectedWorkshops = [];
      await loadProducts();
      renderSelectedProduct();
      renderWorkshops();
      showMessage("ok", "Готово", "Продукция удалена.");
    } catch (err) {
      showMessage("err", "Ошибка удаления", err.message, err.details || "");
    } finally {
      setBusy(false);
    }
  }

  function openWorkshopForm(mode, workshopItem = null) {
    if (!state.selectedProduct) {
      showMessage("warn", "Нет выбранной продукции", "Сначала выберите карточку продукции.");
      return;
    }
    state.workshopFormMode = mode;
    state.editingWorkshop = workshopItem;
    app.workshopFormBadge.textContent = mode === "create" ? "ДОБАВИТЬ" : "ИЗМЕНИТЬ";
    setBadge(app.workshopFormBadge, mode === "create" ? "ok" : "warn");
    app.workshopFormTitle.textContent = mode === "create" ? "Добавление цеха к продукции" : "Редактирование времени в цехе";
    app.workshopFormFields.innerHTML = `
      <div class="field field--full">
        <label for="workshop_id_input">Цех</label>
        <select id="workshop_id_input"><option value="">Выберите цех</option>${getWorkshopOptions(workshopItem?.workshopId, mode === "edit")}</select>
        <div class="errtxt" id="err_workshop_id"></div>
      </div>
      <div class="field field--full">
        <label for="workshop_hours_input">Время в цехе (часы)</label>
        <input id="workshop_hours_input" type="number" min="0" step="0.01" value="${workshopItem ? escapeHtml(workshopItem.manufactureHours) : ""}" />
        <div class="errtxt" id="err_workshop_hours"></div>
      </div>`;
    openModal(app.workshopModal);
  }

  function validateWorkshopForm() {
    const workshopId = $("workshop_id_input")?.value || "";
    const hours = $("workshop_hours_input")?.value.trim() || "";
    $("err_workshop_id").textContent = workshopId ? "" : "Выберите цех.";
    $("err_workshop_hours").textContent = hours ? "" : "Укажите время изготовления.";
    if (hours && Number(hours) < 0) $("err_workshop_hours").textContent = "Время не может быть отрицательным.";
    const hasErrors = $("err_workshop_id").textContent || $("err_workshop_hours").textContent;
    if (hasErrors) return null;
    return { workshopId: Number(workshopId), manufactureHours: Number(hours) };
  }

  async function submitWorkshopForm() {
    const payload = validateWorkshopForm();
    if (!payload) {
      showMessage("warn", "Проверьте форму", "Исправьте ошибки в данных цеха.");
      return;
    }

    try {
      setBusy(true);
      if (state.workshopFormMode === "create") {
        await http("POST", apiUrl("/ProductWorkshop"), {
          ProductId: state.selectedProduct.productId,
          WorkshopId: payload.workshopId,
          ManufactureHours: payload.manufactureHours,
        }, true);
        log(`Добавлен цех ${payload.workshopId} для продукции ${state.selectedProduct.productId}.`);
      } else {
        const patch = [{ op: "replace", path: "/ManufactureHours", value: payload.manufactureHours }];
        await http("PATCH", apiUrl(`/ProductWorkshop/${state.selectedProduct.productId}/${state.editingWorkshop.workshopId}`), patch, true);
        log(`Обновлено время цеха ${state.editingWorkshop.workshopId} для продукции ${state.selectedProduct.productId}.`);
      }
      closeModal(app.workshopModal);
      await loadProducts();
      await selectProduct(state.selectedProduct.productId);
      showMessage("ok", "Готово", "Данные по цеху сохранены.");
    } catch (err) {
      showMessage("err", "Ошибка сохранения цеха", err.message, err.details || "");
    } finally {
      setBusy(false);
    }
  }

  async function deleteWorkshop(item) {
    const ok = await confirmDialog("Удаление связи с цехом", `Удалить цех «${item.workshopName}» из продукции «${state.selectedProduct.name}»?`);
    if (!ok) return;
    try {
      setBusy(true);
      await http("DELETE", apiUrl(`/ProductWorkshop/${item.productId}/${item.workshopId}`), undefined, false);
      log(`Удален цех ${item.workshopId} у продукции ${item.productId}.`);
      await loadProducts();
      await selectProduct(state.selectedProduct.productId);
      showMessage("ok", "Готово", "Цех удален из продукции.");
    } catch (err) {
      showMessage("err", "Ошибка удаления цеха", err.message, err.details || "");
    } finally {
      setBusy(false);
    }
  }

  async function calculateMaterial() {
    const payload = {
      ProductTypeId: Number(app.calcProductType?.value || 0),
      MaterialTypeId: Number(app.calcMaterialType?.value || 0),
      ProductCount: Number(app.calcCount?.value || 0),
      Param1: Number(app.calcParam1?.value || 0),
      Param2: Number(app.calcParam2?.value || 0),
    };

    if (!payload.ProductTypeId || !payload.MaterialTypeId || payload.ProductCount <= 0 || payload.Param1 <= 0 || payload.Param2 <= 0) {
      showMessage("warn", "Недостаточно данных", "Для расчета заполните все поля положительными значениями.");
      return;
    }

    try {
      setBusy(true);
      const result = await http("POST", apiUrl("/Product/calculate-material"), payload, true);
      const value = result?.result ?? result?.Result ?? -1;
      app.calcResult.textContent = `Результат расчета: ${value}`;
      log("Выполнен расчет потребности в сырье.");
    } catch (err) {
      showMessage("err", "Ошибка расчета", err.message, err.details || "");
    } finally {
      setBusy(false);
    }
  }

  function openDetails() {
    if (!state.selectedProduct) {
      showMessage("warn", "Нет выбранной продукции", "Сначала выберите карточку продукции.");
      return;
    }
    showMessage("ok", state.selectedProduct.name, JSON.stringify(state.selectedProduct, null, 2));
  }

  function toggleLogs(showLogs) {
    if (!app.viewLogs) return;
    app.viewLogs.classList.toggle("hidden", !showLogs);
    app.navProducts?.classList.toggle("active", !showLogs);
    app.navLogs?.classList.toggle("active", showLogs);
  }

  function initLoginPage() {
    wireModalClose(shared.msgModal);
    shared.btnDoLogin?.addEventListener("click", () => doLogin(false));
    shared.btnRegister?.addEventListener("click", () => doLogin(true));
    shared.btnGoApp?.addEventListener("click", () => goToApp());
    shared.loginUser?.addEventListener("keydown", (ev) => { if (ev.key === "Enter") doLogin(false); });
    shared.loginPass?.addEventListener("keydown", (ev) => { if (ev.key === "Enter") doLogin(false); });
    if (state.token) goToApp();
  }

  async function initAppPage() {
    if (!state.token) { goToLogin(); return; }

    wireModalClose(shared.msgModal);
    wireModalClose(app.tokenModal);
    wireModalClose(app.formModal);
    wireModalClose(app.workshopModal);
    wireModalClose(app.confirmModal, () => {
      const resolve = state.confirmResolve;
      state.confirmResolve = null;
      if (resolve) resolve(false);
    });

    app.btnConfirmYes?.addEventListener("click", () => {
      closeModal(app.confirmModal);
      const resolve = state.confirmResolve;
      state.confirmResolve = null;
      if (resolve) resolve(true);
    });

    app.btnTokenToggle?.addEventListener("click", () => openModal(app.tokenModal));
    app.btnTokenCopy?.addEventListener("click", async () => {
      try {
        await navigator.clipboard.writeText(state.token || "");
        showMessage("ok", "Готово", "Токен скопирован.");
      } catch {
        showMessage("warn", "Буфер обмена недоступен", "Браузер не разрешил копирование.");
      }
    });

    app.btnLoginPage?.addEventListener("click", goToLogin);
    app.btnLogout?.addEventListener("click", doLogout);
    app.navExit?.addEventListener("click", doLogout);
    app.navProducts?.addEventListener("click", () => toggleLogs(false));
    app.navLogs?.addEventListener("click", () => toggleLogs(true));
    app.btnLogsClear?.addEventListener("click", () => { if (app.log) app.log.textContent = ""; });

    app.searchQ?.addEventListener("input", filterProducts);
    app.btnReload?.addEventListener("click", async () => {
      try {
        setBusy(true);
        await loadProducts();
        if (state.selectedProduct) await selectProduct(state.selectedProduct.productId);
        log("Каталог продукции обновлен.");
      } catch (err) {
        showMessage("err", "Ошибка обновления", err.message, err.details || "");
      } finally {
        setBusy(false);
      }
    });

    app.btnOpenCreate?.addEventListener("click", () => openProductForm("create"));
    app.btnOpenEdit?.addEventListener("click", () => openProductForm("edit"));
    app.btnOpenView?.addEventListener("click", openDetails);
    app.btnOpenDelete?.addEventListener("click", deleteSelectedProduct);
    app.btnFormSubmit?.addEventListener("click", submitProductForm);
    app.btnAddWorkshop?.addEventListener("click", () => openWorkshopForm("create"));
    app.btnRefreshWorkshops?.addEventListener("click", async () => {
      try {
        setBusy(true);
        await loadSelectedProductWorkshops();
        log("Список цехов обновлен.");
      } catch (err) {
        showMessage("err", "Ошибка обновления цехов", err.message, err.details || "");
      } finally {
        setBusy(false);
      }
    });
    app.btnWorkshopSubmit?.addEventListener("click", submitWorkshopForm);
    app.btnCalcMaterial?.addEventListener("click", calculateMaterial);
    app.btnDetailsCopy?.addEventListener("click", async () => {
      if (!state.selectedProduct) {
        showMessage("warn", "Нет выбранной продукции", "Сначала выберите карточку продукции.");
        return;
      }
      try {
        await navigator.clipboard.writeText(JSON.stringify(state.selectedProduct, null, 2));
        showMessage("ok", "Готово", "JSON продукции скопирован.");
      } catch {
        showMessage("warn", "Буфер обмена недоступен", "Браузер не разрешил копирование.");
      }
    });

    try {
      setBusy(true);
      setAuthUI();
      await loadLookups();
      await loadProducts();
      if (state.filteredProducts.length) await selectProduct(state.filteredProducts[0].productId);
      toggleLogs(false);
      log("Приложение загружено.");
    } catch (err) {
      showMessage("err", "Ошибка запуска", err.message, err.details || "");
    } finally {
      setBusy(false);
    }
  }

  if (page === "login") initLoginPage();
  else initAppPage();
})();
