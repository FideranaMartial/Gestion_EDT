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
  let _toastQueue = [];
  let _isProcessingToast = false;

  /**
   * Affiche un toast de notification.
   * @param {string} message   - Message à afficher
   * @param {string} type      - 'success' | 'error' | 'warning' | 'info'
   * @param {string} [title]   - Titre optionnel (remplace le titre par défaut)
   * @param {number} [duration]- Durée en ms (défaut: 4000)
   */
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

    /* Force reflow pour que la transition CSS fonctionne */
    toast.getBoundingClientRect();
    toast.classList.add('show');

    /* Dismiss function */
    const dismiss = () => {
      if (toast._dismissed) return;
      toast._dismissed = true;
      clearTimeout(timer);
      toast.classList.add('hiding');
      toast.classList.remove('show');
      toast.addEventListener('transitionend', () => toast.remove(), { once: true });
      /* Fallback si transitionend ne se déclenche pas */
      setTimeout(() => toast.remove(), 400);
    };

    toast._dismiss = dismiss;

    const timer = setTimeout(dismiss, duration);

    /* Pause au hover */
    toast.addEventListener('mouseenter', () => clearTimeout(timer));
    toast.addEventListener('mouseleave', () => {
      const remaining = duration * 0.5; /* donne un peu de temps au retour */
      setTimeout(dismiss, remaining);
    });
  };


  /* ════════════════════════════════════════
     CONFIRM MODAL
     ════════════════════════════════════════ */

  /**
   * Affiche une modale de confirmation Bootstrap 5.
   * @param {Object} options
   * @param {string} options.title       - Titre de la modale
   * @param {string} options.message     - Corps du message (HTML autorisé)
   * @param {string} [options.confirmText]   - Texte du bouton confirmer (défaut: "Confirmer")
   * @param {string} [options.cancelText]    - Texte du bouton annuler (défaut: "Annuler")
   * @param {string} [options.confirmClass]  - Classe CSS du bouton confirmer (défaut: "btn-danger-emit")
   * @param {string} [options.icon]          - Classe icône Bootstrap Icon
   * @returns {Promise<boolean>} - true si confirmé, false si annulé
   */
  window.showConfirmModal = function (options = {}) {
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

      /* Remplissage du contenu */
      const titleEl   = modal.querySelector('#confirmModalTitle');
      const bodyEl    = modal.querySelector('#confirmModalBody');
      const confirmEl = modal.querySelector('#confirmModalBtn');
      const cancelEl  = modal.querySelector('#cancelModalBtn');

      if (titleEl) {
        titleEl.innerHTML = `<i class="bi ${escapeHtml(icon)}" style="color:var(--warning);margin-right:8px;"></i>${escapeHtml(title)}`;
      }
      if (bodyEl)    bodyEl.innerHTML = message;
      if (confirmEl) {
        confirmEl.textContent = confirmText;
        confirmEl.className   = confirmClass;
      }
      if (cancelEl)  cancelEl.textContent = cancelText;

      /* Instance Bootstrap */
      const bsModal = bootstrap.Modal.getOrCreateInstance(modal);
      bsModal.show();

      /* Handlers */
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

  /**
   * Récupère le token CSRF ASP.NET Core depuis le champ caché.
   * @returns {string} La valeur du token, ou chaîne vide si absent.
   */
  window.getAntiForgeryToken = function () {
    const input = document.querySelector('input[name="__RequestVerificationToken"]');
    if (!input) {
      console.warn('[EMIT] AntiForgeryToken introuvable dans le DOM.');
      return '';
    }
    return input.value;
  };


  /* ════════════════════════════════════════
     FETCH WITH SPINNER
     ════════════════════════════════════════ */

  /**
   * Exécute une promesse fetch en gérant le spinner et l'état disabled du bouton.
   * @param {HTMLButtonElement} button  - Bouton à désactiver pendant la requête
   * @param {Promise}           fetchPromise - La promesse retournée par MockAPI.*()
   * @returns {Promise<any>} - Le résultat de fetchPromise
   */
  window.fetchWithSpinner = async function (button, fetchPromise) {
    if (!button) return fetchPromise;

    /* Sauvegarde du contenu original */
    const originalHTML     = button.innerHTML;
    const originalDisabled = button.disabled;

    /* Extraction du texte visible pour l'aria-label */
    const btnText = button.textContent.trim() || 'Chargement';

    /* Application du spinner */
    button.disabled = true;
    button.innerHTML = `
      <span class="spinner-border spinner-border-sm" role="status" aria-hidden="true" style="width:14px;height:14px;border-width:2px;"></span>
      <span>${escapeHtml(btnText)}</span>
    `;
    button.setAttribute('aria-busy', 'true');

    try {
      const result = await fetchPromise;
      return result;
    } finally {
      /* Restauration du bouton dans tous les cas */
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

      /* Correspondance exacte pour /, sinon prefix match */
      if (linkPath === '/' && (currentPath === '/' || currentPath === '/home' || currentPath === '/home/index')) {
        link.classList.add('active');
      } else if (linkPath !== '/' && currentPath.startsWith(linkPath)) {
        link.classList.add('active');
      }
    });

    /* Fallback : on tente aussi via href */
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

    /* ── Restauration état depuis localStorage ── */
    if (!isMobile()) {
      const collapsed = localStorage.getItem(STORAGE_KEY) === 'true';
      if (collapsed) {
        wrapper.classList.add('sidebar-collapsed');
      }
    }

    /* ── Toggle desktop ── */
    function toggleDesktop() {
      const isCollapsed = wrapper.classList.toggle('sidebar-collapsed');
      localStorage.setItem(STORAGE_KEY, isCollapsed.toString());
      updateToggleIcon();
    }

    /* ── Toggle mobile ── */
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
      const collapsed  = wrapper.classList.contains('sidebar-collapsed');
      const icon       = toggleBtn.querySelector('i');
      if (!icon) return;
      if (collapsed) {
        icon.className = 'bi bi-layout-sidebar-inset-reverse';
      } else {
        icon.className = 'bi bi-layout-sidebar-inset';
      }
    }

    /* ── Événements ── */
    if (toggleBtn) {
      toggleBtn.addEventListener('click', () => {
        if (isMobile()) {
          toggleMobile();
        } else {
          toggleDesktop();
        }
      });
    }

    if (overlay) {
      overlay.addEventListener('click', closeMobile);
    }

    /* Fermeture mobile au clic sur un lien nav */
    sidebar.querySelectorAll('.nav-link').forEach(link => {
      link.addEventListener('click', () => {
        if (isMobile()) closeMobile();
      });
    });

    /* ── Resize handler ── */
    let resizeTimer;
    window.addEventListener('resize', () => {
      clearTimeout(resizeTimer);
      resizeTimer = setTimeout(() => {
        if (!isMobile()) {
          closeMobile();
          document.body.style.overflow = '';
        }
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

    /* On récupère le titre de la page active dans la sidebar */
    const activeLink = document.querySelector('#sidebar .nav-link.active');
    if (activeLink) {
      const labelEl = activeLink.querySelector('.nav-link-label');
      if (labelEl) {
        titleEl.textContent = labelEl.textContent.trim();
      }
    }

    /* Fallback depuis le <title> de la page */
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


  /* ════════════════════════════════════════
     UTILITAIRES PUBLICS
     ════════════════════════════════════════ */

  /**
   * Formate une date ISO en chaîne lisible.
   * @param {string} isoString
   * @param {Object} [opts] - Options Intl.DateTimeFormat
   * @returns {string}
   */
  window.formatDate = function (isoString, opts = {}) {
    if (!isoString) return '—';
    const d = new Date(isoString);
    if (isNaN(d.getTime())) return isoString;
    const defaults = {
      dateStyle: 'short',
      timeStyle: 'short',
    };
    return d.toLocaleString('fr-FR', { ...defaults, ...opts });
  };

  /**
   * Formate une durée entre deux dates ISO en heures/minutes.
   * @param {string} start
   * @param {string} end
   * @returns {string}
   */
  window.formatDuration = function (start, end) {
    const ms      = new Date(end) - new Date(start);
    if (isNaN(ms) || ms < 0) return '—';
    const h  = Math.floor(ms / 3600000);
    const m  = Math.floor((ms % 3600000) / 60000);
    if (h === 0)      return `${m}min`;
    if (m === 0)      return `${h}h`;
    return `${h}h${String(m).padStart(2, '0')}`;
  };

  /**
   * Crée des initiales à partir d'un nom complet.
   * @param {string} fullName
   * @returns {string} 2 lettres majuscules
   */
  window.getInitials = function (fullName) {
    if (!fullName) return '?';
    const parts = fullName.trim().split(/\s+/);
    if (parts.length === 1) return parts[0].substring(0, 2).toUpperCase();
    return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
  };

  /**
   * Gère l'erreur API de manière uniforme.
   * @param {any} err - Erreur retournée par MockAPI
   * @param {string} [context] - Contexte pour le log
   */
  window.handleApiError = function (err, context = '') {
    const code    = err?.code || 500;
    const message = err?.message || 'Une erreur inattendue s\'est produite.';
    console.error(`[EMIT API Error${context ? ' — ' + context : ''}]`, err);

    if (code === 409) {
      window.showToast(message, 'warning', 'Conflit détecté');
    } else if (code === 404) {
      window.showToast(message, 'error', 'Introuvable');
    } else if (code === 400) {
      window.showToast(message, 'warning', 'Données invalides');
    } else {
      window.showToast(message, 'error', 'Erreur serveur');
    }
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

  /* Exposition publique */
  window._emitEscapeHtml = escapeHtml;


  /* ════════════════════════════════════════
     INITIALISATION AU DOMCONTENTLOADED
     ════════════════════════════════════════ */

  document.addEventListener('DOMContentLoaded', () => {
    initSidebar();
    initActiveSidebarLink();
    initTopbarTitle();
    initLogout();

    /* Supprime le page loader si présent */
    const loader = document.getElementById('page-loader');
    if (loader) {
      setTimeout(() => {
        loader.classList.add('fade-out');
        setTimeout(() => loader.remove(), 450);
      }, 200);
    }
  });

})();
