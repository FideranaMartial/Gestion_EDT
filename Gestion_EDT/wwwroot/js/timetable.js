/**
 * timetable.js — EMIT Planning Module
 * FullCalendar v6.1.10 + Bootstrap 5 Popovers
 * Filtres en cascade : Mention → Parcours → Groupe
 * UTILISE LES VRAIES APIs
 */

(function () {
  'use strict';

  /* ─── État des filtres ─────────────────────────────────────── */
  const filters = { mentionId: null, parcoursId: null, groupeId: null };

  /* ─── Couleurs par type de séance (palette EMIT) ──────────── */
  const PALETTE = [
    '#73B9E6', '#5AA8D8', '#34D399', '#FBBF24',
    '#F87171', '#A78BFA', '#FB923C', '#38BDF8',
  ];
  const colorCache = {};
  function getColor(title) {
    if (!colorCache[title]) {
      const idx = Object.keys(colorCache).length % PALETTE.length;
      colorCache[title] = PALETTE[idx];
    }
    return colorCache[title];
  }

  /* ─── Utilitaire debounce ──────────────────────────────────── */
  function debounce(fn, delay) {
    let timer;
    return function (...args) {
      clearTimeout(timer);
      timer = setTimeout(() => fn.apply(this, args), delay);
    };
  }

  /* ─── Détermine la vue selon la largeur ───────────────────── */
  function resolveView() {
    return window.innerWidth < 576 ? 'listWeek' : 'timeGridWeek';
  }

  /* ─── Référence calendrier ────────────────────────────────── */
  let calendar = null;

  /* ─── Récupère le token CSRF ───────────────────────────────── */
  function getAntiForgeryToken() {
    const token = document.querySelector('input[name="__RequestVerificationToken"]');
    return token ? token.value : '';
  }

  /* ─── Peuple le select Mention ────────────────────────────── */
  async function loadMentions() {
    const sel = document.getElementById('filterMention');
    if (!sel) return;
    sel.innerHTML = '<option value="">Toutes les mentions</option>';
    try {
      const response = await fetch('/Mentions/GetAll');
      if (!response.ok) throw new Error('Erreur chargement mentions');
      const mentions = await window.API.getMentions();
      mentions.forEach(m => {
        const opt = document.createElement('option');
        opt.value = m.id;
        opt.textContent = m.nom_mention || m.nom;
        sel.appendChild(opt);
      });
    } catch (err) {
      window.showToast('Impossible de charger les mentions.', 'error');
      console.error(err);
    }
  }

  /* ─── Peuple le select Parcours selon la mention ─────────── */
  async function loadParcours(mentionId) {
    const sel = document.getElementById('filterParcours');
    if (!sel) return;
    sel.innerHTML = '<option value="">Tous les parcours</option>';
    sel.disabled = !mentionId;

    const grpSel = document.getElementById('filterGroupe');
    if (grpSel) {
      grpSel.innerHTML = '<option value="">Tous les groupes</option>';
      grpSel.disabled = true;
    }
    filters.parcoursId = null;
    filters.groupeId = null;

    if (!mentionId) return;
    try {
      // Utilise l'API de PlanningController ou MentionsController
      const response = await fetch(`/Planning/GetParcours?mentionId=${mentionId}`);
      if (!response.ok) throw new Error('Erreur chargement parcours');
      const parcours = await response.json();
      parcours.forEach(p => {
        const opt = document.createElement('option');
        opt.value = p.value;
        opt.textContent = p.label;
        sel.appendChild(opt);
      });
    } catch (err) {
      window.showToast('Impossible de charger les parcours.', 'error');
      console.error(err);
    }
  }

  /* ─── Peuple le select Groupe selon le parcours ─────────── */
  async function loadGroupes(parcoursId) {
    const sel = document.getElementById('filterGroupe');
    if (!sel) return;
    sel.innerHTML = '<option value="">Tous les groupes</option>';
    sel.disabled = !parcoursId;
    filters.groupeId = null;

    if (!parcoursId) return;
    try {
      const response = await fetch(`/Planning/GetGroupes?parcoursId=${parcoursId}`);
      if (!response.ok) throw new Error('Erreur chargement groupes');
      const groupes = await response.json();
      groupes.forEach(g => {
        const opt = document.createElement('option');
        opt.value = g.value;
        opt.textContent = g.label;
        sel.appendChild(opt);
      });
    } catch (err) {
      window.showToast('Impossible de charger les groupes.', 'error');
      console.error(err);
    }
  }

  /* ─── Détruit les popovers actifs ────────────────────────── */
  function destroyPopovers() {
    document.querySelectorAll('[data-bs-toggle="popover"]').forEach(el => {
      const instance = bootstrap.Popover.getInstance(el);
      if (instance) instance.dispose();
    });
  }

  /* ─── Initialise le calendrier FullCalendar ──────────────── */
  function initCalendar() {
    const el = document.getElementById('calendar');
    if (!el) return;

    calendar = new FullCalendar.Calendar(el, {
      locale: 'fr',
      initialView: resolveView(),
      hiddenDays: [0],
      slotMinTime: '07:30:00',
      slotMaxTime: '18:30:00',
      allDaySlot: false,
      nowIndicator: true,
      headerToolbar: {
        left: 'prev,next today',
        center: 'title',
        right: 'timeGridWeek,timeGridDay,listWeek',
      },
      buttonText: {
        today: "Aujourd'hui",
        week: 'Semaine',
        day: 'Jour',
        list: 'Liste',
      },
      height: 'auto',
      expandRows: true,
      slotLabelFormat: { hour: '2-digit', minute: '2-digit', hour12: false },
      eventTimeFormat: { hour: '2-digit', minute: '2-digit', hour12: false },

      /* ── Source d'événements via API réelle ── */
      events: async function (fetchInfo, successCallback, failureCallback) {
        try {
            const seances = await window.API.getSeances({
                mentionId: filters.mentionId,
                parcoursId: filters.parcoursId,
                groupeId: filters.groupeId
            });
          
          const response = await fetch(url);
          if (!response.ok) throw new Error('Erreur chargement séances');
          const events = await response.json();
          successCallback(events);
        } catch (err) {
          failureCallback(err);
          window.showToast('Erreur lors du chargement des séances.', 'error');
          console.error(err);
        }
      },

      eventClick: function(info) {
        // Rediriger vers la page de détail ou d'édition
        if (info.event.id) {
          window.location.href = `/Seances/Edit/${info.event.id}`;
        }
      },

      /* ── Rendu de chaque événement : border-left + popover ── */
      eventDidMount: function (info) {
        const color = info.event.borderColor || '#73B9E6';
        info.el.style.borderLeft = `4px solid ${color}`;
        info.el.style.borderRadius = '6px';
        info.el.style.fontSize = '0.78rem';
        info.el.style.padding = '2px 4px';
        info.el.style.cursor = 'pointer';

        const props = info.event.extendedProps;
        const content = `
          <div style="font-size:0.82rem;line-height:1.5;min-width:180px;">
            <div><i class="bi bi-door-open-fill me-1" style="color:#73B9E6"></i><strong>${props.salle || '—'}</strong></div>
            <div><i class="bi bi-person-fill me-1" style="color:#34D399"></i>${props.enseignant || '—'}</div>
            <div><i class="bi bi-people-fill me-1" style="color:#FBBF24"></i>${props.groupe || '—'}</div>
            ${props.mention ? `<div><i class="bi bi-building me-1" style="color:#A78BFA"></i>${props.mention}</div>` : ''}
          </div>`;

        const popover = new bootstrap.Popover(info.el, {
          trigger: 'hover focus',
          placement: 'top',
          html: true,
          title: `<strong>${info.event.title}</strong>`,
          content: content,
          customClass: 'emit-popover',
          container: 'body',
        });

        info.el.addEventListener('remove', () => popover.dispose(), { once: true });
      },

      eventWillUnmount: function () {
        destroyPopovers();
      },
    });

    calendar.render();
  }

  /* ─── Gestion du resize responsive ──────────────────────── */
  const handleResize = debounce(function () {
    if (!calendar) return;
    const newView = resolveView();
    const currentView = calendar.view.type;
    if (newView !== currentView) {
      calendar.changeView(newView);
    }
  }, 300);

  /* ─── Attachement des listeners de filtres ───────────────── */
  function bindFilters() {
    const mentionSel = document.getElementById('filterMention');
    const parcoursSel = document.getElementById('filterParcours');
    const groupeSel = document.getElementById('filterGroupe');

    if (mentionSel) {
      mentionSel.addEventListener('change', async function () {
        filters.mentionId = this.value || null;
        await loadParcours(filters.mentionId);
        if (calendar) calendar.refetchEvents();
      });
    }

    if (parcoursSel) {
      parcoursSel.addEventListener('change', async function () {
        filters.parcoursId = this.value || null;
        await loadGroupes(filters.parcoursId);
        if (calendar) calendar.refetchEvents();
      });
    }

    if (groupeSel) {
      groupeSel.addEventListener('change', function () {
        filters.groupeId = this.value || null;
        if (calendar) calendar.refetchEvents();
      });
    }
  }

  /* ─── Bouton "Nouvelle séance" ───────────────────────────── */
  function bindNewSeanceBtn() {
    const btn = document.getElementById('btnNewSeance');
    if (btn) {
      btn.addEventListener('click', function () {
        window.location.href = '/Seances/Create';
      });
    }
  }

  /* ─── CSS pour les popovers ──────────────────────────────── */
  function injectPopoverStyles() {
    if (document.getElementById('emit-popover-style')) return;
    const style = document.createElement('style');
    style.id = 'emit-popover-style';
    style.textContent = `
      .emit-popover .popover-body { background: #1F4E79; color: #F5F7FA; border-radius: 0 0 8px 8px; }
      .emit-popover .popover-header { background: #172554; color: #73B9E6; border-radius: 8px 8px 0 0; border-bottom: 1px solid #73B9E633; font-size: 0.85rem; }
      .emit-popover.bs-popover-top .popover-arrow::after { border-top-color: #1F4E79; }
      .emit-popover.bs-popover-bottom .popover-arrow::after { border-bottom-color: #172554; }
      .emit-popover { border: 1px solid #73B9E644; box-shadow: 0 8px 24px rgba(0,0,0,0.4); }
      .fc-event { cursor: pointer; }
      .fc-timegrid-slot { border-color: rgba(115,185,230,0.08) !important; }
      .fc-col-header-cell { background: rgba(26,58,92,0.7); }
      .fc-scrollgrid { border-color: rgba(115,185,230,0.15) !important; }
      .fc-timegrid-now-indicator-line { border-color: #F87171 !important; }
      .fc-button-primary { background-color: #1F4E79 !important; border-color: #73B9E6 !important; color: #F5F7FA !important; }
      .fc-button-primary:hover { background-color: #73B9E6 !important; color: #172554 !important; }
      .fc-button-primary.fc-button-active { background-color: #73B9E6 !important; color: #172554 !important; font-weight: 600; }
      .fc-toolbar-title { color: #F5F7FA !important; font-family: 'DM Sans', sans-serif !important; font-weight: 600; }
      .fc-col-header-cell-cushion, .fc-timegrid-slot-label-cushion, .fc-list-event-title a { color: #94A3B8 !important; font-family: 'DM Sans', sans-serif !important; }
      .fc-list-event:hover td { background: rgba(115,185,230,0.1) !important; }
    `;
    document.head.appendChild(style);
  }

  /* ─── Point d'entrée ─────────────────────────────────────── */
  document.addEventListener('DOMContentLoaded', async function () {
    injectPopoverStyles();
    bindFilters();
    bindNewSeanceBtn();
    await loadMentions();
    initCalendar();
    window.addEventListener('resize', handleResize);
  });

})();