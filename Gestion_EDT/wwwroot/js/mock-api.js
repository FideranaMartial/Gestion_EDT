// ═══════════════════════════════════════
// FICHIER : /wwwroot/js/mock-api.js
// Application : EMIT — Gestion Emploi du Temps
// API Mockée pour le développement frontend
// ═══════════════════════════════════════

(function () {
  'use strict';

  /* ─── CONFIGURATION ─── */
  const config = {
    simulateErrors: false,     // Désactivé pour éviter les erreurs aléatoires pendant le dev
    errorRate: 0.10,
    latency: 200,              // Réponse rapide
  };

  /* ─── DONNÉES INITIALES ─── */
  let _mentions = [
    { id: 1, nom: 'Informatique',   niveau: 'Master'   },
    { id: 2, nom: 'Mathématiques',  niveau: 'Licence'  },
    { id: 3, nom: 'Commerce',       niveau: 'Licence'  },
    { id: 4, nom: 'Physique',       niveau: 'Doctorat' },
  ];

  let _enseignants = [
    { id: 1, nom: 'DUPONT',  prenom: 'Marie',  matieres: ['Algorithmique', 'POO'],               disponible: true  },
    { id: 2, nom: 'RAKOTO',  prenom: 'Jean',   matieres: ['Réseaux', 'Sécurité'],                disponible: true  },
    { id: 3, nom: 'MARTIN',  prenom: 'Sophie', matieres: ['Algèbre', 'Analyse'],                 disponible: false },
    { id: 4, nom: 'ANDRIA',  prenom: 'Paul',   matieres: ['Base de données', 'SQL'],             disponible: true  },
  ];

  let _seances = [
    {
      id: 1,
      title: 'Algorithmique',
      start: '2025-06-09T08:00:00',
      end:   '2025-06-09T10:00:00',
      salle: 'Salle A101',
      professeur: 'Marie DUPONT',
      enseignantId: 1,
      groupe: 'Groupe A',
      groupeId: 1,
      type: 'Cours',
      couleur: '#73B9E6',
    },
    {
      id: 2,
      title: 'POO Java',
      start: '2025-06-09T10:30:00',
      end:   '2025-06-09T12:30:00',
      salle: 'Labo Info',
      professeur: 'Marie DUPONT',
      enseignantId: 1,
      groupe: 'Groupe B',
      groupeId: 2,
      type: 'TP',
      couleur: '#34D399',
    },
  ];

  let _parcours = [
    { id: 1, mentionId: 1, nom: 'Parcours Développement' },
    { id: 2, mentionId: 1, nom: 'Parcours Systèmes' },
    { id: 3, mentionId: 2, nom: 'Parcours Analyse' },
    { id: 4, mentionId: 3, nom: 'Parcours Gestion' },
  ];

  let _groupes = [
    { id: 1, parcoursId: 1, nom: 'Groupe A' },
    { id: 2, parcoursId: 1, nom: 'Groupe B' },
    { id: 3, parcoursId: 2, nom: 'Groupe C' },
    { id: 4, parcoursId: 3, nom: 'Groupe D' },
    { id: 5, parcoursId: 4, nom: 'Groupe E' },
  ];

  let _kpis = {
    totalMentions: 4,      // correspond au nombre réel de mentions
    seancesSemaine: 2,
    tauxOccupation: 78,
    heuresEnseignees: 8,
  };

  let _seancesParJour = {
    labels: ['Lundi', 'Mardi', 'Mercredi', 'Jeudi', 'Vendredi'],
    data:   [2, 0, 0, 0, 0],
  };

  let _nextId = 100;

  function _generateId() {
    return ++_nextId;
  }

  function _delay(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
  }

  function _maybeError() {
    if (config.simulateErrors && Math.random() < config.errorRate) {
      throw { code: 500, message: 'Erreur simulée du serveur. Veuillez réessayer.' };
    }
  }

  async function _simulate(fn) {
    await _delay(config.latency);
    _maybeError();
    return fn();
  }

  function _notFound(entity, id) {
    throw { code: 404, message: `${entity} avec l'id ${id} introuvable.` };
  }

  function _conflict(message) {
    throw { code: 409, message };
  }

  /* ─── PUBLIC API ─── */
  const MockAPI = {

    config,

    // ========== MENTIONS ==========
    getMentions() {
      return _simulate(() => JSON.parse(JSON.stringify(_mentions)));
    },

    getMentionById(id) {
      return _simulate(() => {
        const m = _mentions.find(x => x.id === id);
        if (!m) _notFound('Mention', id);
        return JSON.parse(JSON.stringify(m));
      });
    },

    getParcours(mentionId) {
      return _simulate(() => {
        if (!mentionId) return [];
        return JSON.parse(JSON.stringify(_parcours.filter(p => p.mentionId === parseInt(mentionId, 10))));
      });
    },

    getGroupes(parcoursId) {
      return _simulate(() => {
        if (!parcoursId) return [];
        return JSON.parse(JSON.stringify(_groupes.filter(g => g.parcoursId === parseInt(parcoursId, 10))));
      });
    },

    createMention(data) {
      return _simulate(() => {
        if (!data.nom || !data.nom.trim()) {
          throw { code: 400, message: 'Le nom de la mention est requis.' };
        }
        if (!data.niveau || !data.niveau.trim()) {
          throw { code: 400, message: 'Le niveau est requis.' };
        }
        const exists = _mentions.find(m =>
          m.nom.toLowerCase() === data.nom.trim().toLowerCase() &&
          m.niveau === data.niveau
        );
        if (exists) _conflict(`Une mention "${data.nom}" (${data.niveau}) existe déjà.`);

        const newMention = {
          id: _generateId(),
          nom: data.nom.trim(),
          niveau: data.niveau.trim(),
        };
        _mentions.push(newMention);
        _kpis.totalMentions = _mentions.length;
        return JSON.parse(JSON.stringify(newMention));
      });
    },

    updateMention(id, data) {
      return _simulate(() => {
        const idx = _mentions.findIndex(x => x.id === id);
        if (idx === -1) _notFound('Mention', id);
        if (!data.nom || !data.nom.trim()) {
          throw { code: 400, message: 'Le nom de la mention est requis.' };
        }
        _mentions[idx] = {
          ..._mentions[idx],
          nom:    data.nom.trim(),
          niveau: data.niveau ? data.niveau.trim() : _mentions[idx].niveau,
        };
        return JSON.parse(JSON.stringify(_mentions[idx]));
      });
    },

    deleteMention(id) {
      return _simulate(() => {
        const idx = _mentions.findIndex(x => x.id === id);
        if (idx === -1) _notFound('Mention', id);
        _mentions.splice(idx, 1);
        _kpis.totalMentions = _mentions.length;
        return { success: true, id };
      });
    },

    // ========== ENSEIGNANTS ==========
    getEnseignants() {
      return _simulate(() => JSON.parse(JSON.stringify(_enseignants)));
    },

    getEnseignantById(id) {
      return _simulate(() => {
        const e = _enseignants.find(x => x.id === id);
        if (!e) _notFound('Enseignant', id);
        return JSON.parse(JSON.stringify(e));
      });
    },

    // ========== SÉANCES ==========
    getSeances(filters = {}) {
      return _simulate(() => {
        let result = JSON.parse(JSON.stringify(_seances));
        if (filters.enseignantId) {
          result = result.filter(s => s.enseignantId === filters.enseignantId);
        }
        if (filters.groupeId) {
          result = result.filter(s => s.groupeId === filters.groupeId);
        }
        return result;
      });
    },

    createSeance(data) {
      return _simulate(() => {
        if (!data.date || !data.heureDebut || !data.heureFin) {
          throw { code: 400, message: 'Date et horaires de la séance sont requis.' };
        }
        if (!data.salleId || !data.enseignantId) {
          throw { code: 400, message: 'Salle et enseignant sont requis.' };
        }
        if (!data.libelle || !data.libelle.trim()) {
          throw { code: 400, message: 'Le libellé de la séance est requis.' };
        }

        const enseignant = _enseignants.find(e => e.id === parseInt(data.enseignantId, 10));
        if (!enseignant) {
          throw { code: 404, message: 'Enseignant introuvable.' };
        }

        const salleNames = {
          1: 'Salle A101',
          2: 'Salle A102',
          3: 'Labo Informatique',
          4: 'Amphi 200',
        };

        const timeStart = `${data.date}T${data.heureDebut}:00`;
        const timeEnd = `${data.date}T${data.heureFin}:00`;

        const seance = {
          id: _generateId(),
          title: data.libelle.trim(),
          start: timeStart,
          end: timeEnd,
          salle: salleNames[data.salleId] || `Salle ${data.salleId}`,
          professeur: `${enseignant.prenom} ${enseignant.nom}`,
          enseignantId: parseInt(data.enseignantId, 10),
          groupe: 'Groupe non défini',
          groupeId: null,
          type: 'Cours',
          couleur: '#73B9E6',
          success: true,
        };
        _seances.push(seance);
        return seance;
      });
    },

    checkConflit(data) {
      return _simulate(() => {
        // Simulation : pas de conflit par défaut
        return { hasConflit: false, message: null, type: null };
      });
    },

    // ========== KPIs ==========
    getKPIs() {
      // Met à jour le nombre total de mentions dynamiquement
      _kpis.totalMentions = _mentions.length;
      return _simulate(() => JSON.parse(JSON.stringify(_kpis)));
    },

    getSeancesParJour() {
      return _simulate(() => {
        const jours = ['Lundi', 'Mardi', 'Mercredi', 'Jeudi', 'Vendredi'];
        const today = new Date();
        const startOfWeek = new Date(today);
        startOfWeek.setDate(today.getDate() - today.getDay() + 1); // Lundi
        const data = jours.map((_, idx) => {
          const dayStart = new Date(startOfWeek);
          dayStart.setDate(startOfWeek.getDate() + idx);
          dayStart.setHours(0, 0, 0, 0);
          const dayEnd = new Date(dayStart);
          dayEnd.setHours(23, 59, 59, 999);
          return _seances.filter(s => {
            const sDate = new Date(s.start);
            return sDate >= dayStart && sDate <= dayEnd;
          }).length;
        });
        return { labels: jours, data };
      });
    },
  };

  /* ─── EXPOSITION GLOBALE ─── */
  window.MockAPI = MockAPI;
  console.log('[EMIT] MockAPI initialisé avec succès', { mentions: _mentions.length });
})();