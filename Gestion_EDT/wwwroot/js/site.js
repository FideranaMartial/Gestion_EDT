// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
// ═══════════════════════════════════════
// FICHIER : /wwwroot/js/site.js
// Application : EMIT — Gestion Emploi du Temps
// ═══════════════════════════════════════

(function () {
  'use strict';

  /* ════════════════════════════════════════
     TOAST SYSTEM
     ════════════════════════════════════════ */

  const TOAST_ICONS = {
    success: 'bi-check-circle-fill',
    error:   'bi-x-circle-fill',
    warning: 'bi-exclamation-triangle-fill',
    info:    'bi-info-circle-fill',
  };

  const TOAST_TITLES = {
    success: 'Succès',
    error:   'Erreur',
    warning: 'Avertissement',
    info:    'Information',
  };

  const TOAST_DURATION = 4000; // ms

  window.showToast = function (message, type = 'info', title = null, duration = TOAST_DURATION) {
    const container = document.getElementById('toast-container');
    if (!container) {
      console.warn('[EMIT] #toast-container introuvable.');
      return;
    }

    const toast = document.createElement('div');
    const resolvedType = ['success', 'error', 'warning', 'info'].includes(type) ? type : 'info';

    toast.className = `toast-emit ${resolvedType}`;
    toast.setAttribute('role', 'alert');
    toast.setAttribute('aria-live', 'polite');
    toast.innerHTML = `
      <i class="bi ${TOAST_ICONS[resolvedType]} toast-emit-icon"></i>
      <div class="toast-emit-body">
        <div class="toast-emit-title">${escapeHtml(title || TOAST_TITLES[resolvedType])}</div>
        <div class="toast-emit-message">${escapeHtml(message)}</div>
      </div>
      <button
        type="button"
        style="background:none;border:none;color:var(--text-muted);cursor:pointer;padding:2px 4px;margin-left:8px;font-size:14px;align-self:flex-start;"
        aria-label="Fermer"
        onclick="this.closest('.toast-emit')._dismiss()"
      >
        <i class="bi bi-x-lg"></i>
      </button>
    `;

    container.appendChild(toast);
    toast.getBoundingClientRect();
    toast.classList.add('show');

    const dismiss = () => {
      if (toast._dismissed) return;
      toast._dismissed = true;
      clearTimeout(timer);
      toast.classList.add('hiding');
      toast.classList.remove('show');
      toast.addEventListener('transitionend', () => toast.remove(), { once: true });
      setTimeout(() => toast.remove(), 400);
    };
    toast._dismiss = dismiss;
    const timer = setTimeout(dismiss, duration);

    toast.addEventListener('mouseenter', () => clearTimeout(timer));
    toast.addEventListener('mouseleave', () => {
      setTimeout(dismiss, duration * 0.5);
    });
  };


  /* ════════════════════════════════════════
     CONFIRM MODAL
     ════════════════════════════════════════ */

  window.showConfirmModal = function (options = {}) {

    // Dans site.js, au début de showConfirmModal
if (typeof bootstrap === 'undefined' || !bootstrap.Modal) {
    console.error('Bootstrap Modal non disponible');
    // Fallback
    const result = confirm(message);
    resolve(result);
    return;
}

    return new Promise((resolve) => {
      const modal = document.getElementById('confirmModal');
      if (!modal) {
        console.warn('[EMIT] #confirmModal introuvable.');
        resolve(false);
        return;
      }

      const {
        title       = 'Confirmation',
        message     = 'Êtes-vous sûr de vouloir effectuer cette action ?',
        confirmText = 'Confirmer',
        cancelText  = 'Annuler',
        confirmClass = 'btn-danger-emit',
        icon        = 'bi-exclamation-triangle-fill',
      } = options;

      const titleEl   = modal.querySelector('#confirmModalTitle');
      const bodyEl    = modal.querySelector('#confirmModalBody');
      const confirmEl = modal.querySelector('#confirmModalBtn');
      const cancelEl  = modal.querySelector('#cancelModalBtn');

      if (titleEl) titleEl.innerHTML = `<i class="bi ${escapeHtml(icon)}" style="color:var(--warning);margin-right:8px;"></i>${escapeHtml(title)}`;
      if (bodyEl)    bodyEl.innerHTML = message;
      if (confirmEl) {
        confirmEl.textContent = confirmText;
        confirmEl.className   = confirmClass;
      }
      if (cancelEl)  cancelEl.textContent = cancelText;

      const bsModal = bootstrap.Modal.getOrCreateInstance(modal);
      bsModal.show();

      let handled = false;

      function onConfirm() {
        if (handled) return;
        handled = true;
        cleanup();
        bsModal.hide();
        resolve(true);
      }
      function onCancel() {
        if (handled) return;
        handled = true;
        cleanup();
        bsModal.hide();
        resolve(false);
      }
      function onHide() {
        if (!handled) {
          handled = true;
          resolve(false);
        }
        cleanup();
      }
      function cleanup() {
        if (confirmEl) confirmEl.removeEventListener('click', onConfirm);
        if (cancelEl)  cancelEl.removeEventListener('click', onCancel);
        modal.removeEventListener('hidden.bs.modal', onHide);
      }

      if (confirmEl) confirmEl.addEventListener('click', onConfirm);
      if (cancelEl)  cancelEl.addEventListener('click', onCancel);
      modal.addEventListener('hidden.bs.modal', onHide, { once: true });
    });
  };


  /* ════════════════════════════════════════
     ANTI-FORGERY TOKEN
     ════════════════════════════════════════ */

  window.getAntiForgeryToken = function () {
    const input = document.querySelector('input[name="__RequestVerificationToken"]');
    if (!input) return '';
    return input.value;
  };


  /* ════════════════════════════════════════
     FETCH WITH SPINNER
     ════════════════════════════════════════ */

  window.fetchWithSpinner = async function (button, fetchPromise) {
    if (!button) return fetchPromise;
    const originalHTML     = button.innerHTML;
    const originalDisabled = button.disabled;
    const btnText = button.textContent.trim() || 'Chargement';
    button.disabled = true;
    button.innerHTML = `<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true" style="width:14px;height:14px;border-width:2px;"></span><span>${escapeHtml(btnText)}</span>`;
    button.setAttribute('aria-busy', 'true');
    try {
      const result = await fetchPromise;
      return result;
    } finally {
      button.innerHTML  = originalHTML;
      button.disabled   = originalDisabled;
      button.removeAttribute('aria-busy');
    }
  };


  /* ════════════════════════════════════════
     SIDEBAR — LIEN ACTIF
     ════════════════════════════════════════ */

  function initActiveSidebarLink() {
    const currentPath = window.location.pathname.toLowerCase();
    const navLinks    = document.querySelectorAll('#sidebar .nav-link[data-path]');
    navLinks.forEach(link => {
      link.classList.remove('active');
      const linkPath = (link.dataset.path || '').toLowerCase();
      if (linkPath === '/' && (currentPath === '/' || currentPath === '/home' || currentPath === '/home/index')) {
        link.classList.add('active');
      } else if (linkPath !== '/' && currentPath.startsWith(linkPath)) {
        link.classList.add('active');
      }
    });
    if (!document.querySelector('#sidebar .nav-link.active')) {
      const allLinks = document.querySelectorAll('#sidebar .nav-link[href]');
      let bestMatch  = null;
      let bestLen    = 0;
      allLinks.forEach(link => {
        const href = link.getAttribute('href');
        if (!href || href === '#') return;
        const hrefPath = href.split('?')[0].toLowerCase();
        if (currentPath.startsWith(hrefPath) && hrefPath.length > bestLen) {
          bestLen   = hrefPath.length;
          bestMatch = link;
        }
      });
      if (bestMatch) bestMatch.classList.add('active');
    }
  }


  /* ════════════════════════════════════════
     SIDEBAR — TOGGLE COLLAPSE & MOBILE
     ════════════════════════════════════════ */

  function initSidebar() {
    const wrapper     = document.getElementById('app-wrapper');
    const sidebar     = document.getElementById('sidebar');
    const toggleBtn   = document.getElementById('sidebar-toggle-btn');
    const overlay     = document.getElementById('sidebar-overlay');
    const STORAGE_KEY = 'emit_sidebar_collapsed';

    if (!wrapper || !sidebar) return;

    const isMobile = () => window.innerWidth <= 576;

    if (!isMobile()) {
      const collapsed = localStorage.getItem(STORAGE_KEY) === 'true';
      if (collapsed) wrapper.classList.add('sidebar-collapsed');
    }

    function toggleDesktop() {
      wrapper.classList.toggle('sidebar-collapsed');
      localStorage.setItem(STORAGE_KEY, wrapper.classList.contains('sidebar-collapsed').toString());
      updateToggleIcon();
    }

    function toggleMobile() {
      const isOpen = sidebar.classList.toggle('mobile-open');
      if (overlay) overlay.classList.toggle('active', isOpen);
      document.body.style.overflow = isOpen ? 'hidden' : '';
    }

    function closeMobile() {
      sidebar.classList.remove('mobile-open');
      if (overlay) overlay.classList.remove('active');
      document.body.style.overflow = '';
    }

    function updateToggleIcon() {
      if (!toggleBtn) return;
      const icon = toggleBtn.querySelector('i');
      if (!icon) return;
      if (wrapper.classList.contains('sidebar-collapsed')) {
        icon.className = 'bi bi-layout-sidebar-inset-reverse';
      } else {
        icon.className = 'bi bi-layout-sidebar-inset';
      }
    }

    if (toggleBtn) {
      toggleBtn.addEventListener('click', () => {
        if (isMobile()) toggleMobile();
        else toggleDesktop();
      });
    }

    if (overlay) overlay.addEventListener('click', closeMobile);
    sidebar.querySelectorAll('.nav-link').forEach(link => {
      link.addEventListener('click', () => { if (isMobile()) closeMobile(); });
    });

    let resizeTimer;
    window.addEventListener('resize', () => {
      clearTimeout(resizeTimer);
      resizeTimer = setTimeout(() => {
        if (!isMobile()) closeMobile();
        else document.body.style.overflow = '';
      }, 150);
    });

    updateToggleIcon();
  }


  /* ════════════════════════════════════════
     PAGE TITLE DANS LA TOPBAR
     ════════════════════════════════════════ */

  function initTopbarTitle() {
    const titleEl = document.getElementById('topbar-page-title');
    if (!titleEl) return;
    const activeLink = document.querySelector('#sidebar .nav-link.active');
    if (activeLink) {
      const labelEl = activeLink.querySelector('.nav-link-label');
      if (labelEl) titleEl.textContent = labelEl.textContent.trim();
    }
    if (!titleEl.textContent.trim()) {
      const pageTitle = document.title;
      if (pageTitle) {
        const parts = pageTitle.split('–').map(s => s.trim());
        titleEl.textContent = parts[0] || 'EMIT';
      }
    }
  }


  /* ════════════════════════════════════════
     DÉCONNEXION
     ════════════════════════════════════════ */

  function initLogout() {
    const logoutBtn = document.getElementById('logout-btn');
    if (!logoutBtn) return;
    logoutBtn.addEventListener('click', async (e) => {
      e.preventDefault();
      const confirmed = await window.showConfirmModal({
        title:       'Déconnexion',
        message:     'Vous êtes sur le point de vous déconnecter.',
        confirmText: 'Se déconnecter',
        cancelText:  'Rester connecté',
        confirmClass: 'btn-danger-emit',
        icon:         'bi-box-arrow-right',
      });
      if (confirmed) {
        const href = logoutBtn.getAttribute('href') || logoutBtn.dataset.href;
        if (href && href !== '#') window.location.href = href;
        else window.showToast('Déconnexion simulée (mock).', 'info');
      }
    });
  }


  /* ════════════════════════════════════════
     UTILITAIRES PUBLICS
     ════════════════════════════════════════ */

  window.formatDate = function (isoString, opts = {}) {
    if (!isoString) return '—';
    const d = new Date(isoString);
    if (isNaN(d.getTime())) return isoString;
    return d.toLocaleString('fr-FR', { dateStyle: 'short', timeStyle: 'short', ...opts });
  };

  window.formatDuration = function (start, end) {
    const ms = new Date(end) - new Date(start);
    if (isNaN(ms) || ms < 0) return '—';
    const h = Math.floor(ms / 3600000);
    const m = Math.floor((ms % 3600000) / 60000);
    if (h === 0) return `${m}min`;
    if (m === 0) return `${h}h`;
    return `${h}h${String(m).padStart(2, '0')}`;
  };

  window.getInitials = function (fullName) {
    if (!fullName) return '?';
    const parts = fullName.trim().split(/\s+/);
    if (parts.length === 1) return parts[0].substring(0, 2).toUpperCase();
    return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
  };

  window.handleApiError = function (err, context = '') {
    const code    = err?.code || 500;
    const message = err?.message || 'Une erreur inattendue s\'est produite.';
    console.error(`[EMIT API Error${context ? ' — ' + context : ''}]`, err);
    if (code === 409) window.showToast(message, 'warning', 'Conflit détecté');
    else if (code === 404) window.showToast(message, 'error', 'Introuvable');
    else if (code === 400) window.showToast(message, 'warning', 'Données invalides');
    else window.showToast(message, 'error', 'Erreur serveur');
  };


  /* ════════════════════════════════════════
     HELPER PRIVÉ : ESCAPE HTML
     ════════════════════════════════════════ */

  function escapeHtml(str) {
    if (typeof str !== 'string') return String(str ?? '');
    return str
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;')
      .replace(/'/g, '&#039;');
  }

  window._emitEscapeHtml = escapeHtml;


  /* ════════════════════════════════════════
     INITIALISATION AU DOMCONTENTLOADED
     ════════════════════════════════════════ */

  document.addEventListener('DOMContentLoaded', () => {
    initSidebar();
    initActiveSidebarLink();
    initTopbarTitle();
    initLogout();
    const loader = document.getElementById('page-loader');
    if (loader) {
      setTimeout(() => {
        loader.classList.add('fade-out');
        setTimeout(() => loader.remove(), 450);
      }, 200);
    }
  });
})();