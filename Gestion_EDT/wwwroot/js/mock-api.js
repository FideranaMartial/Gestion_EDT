// ═══════════════════════════════════════
// FICHIER : /wwwroot/js/mock-api.js
// Application : EMIT — Gestion Emploi du Temps
// ═══════════════════════════════════════

(function () {
  'use strict';

  /* ─── CONFIGURATION ─── */
  const config = {
    simulateErrors: true,
    errorRate: 0.10,      // 10% d'erreurs aléatoires
    latency: 300,         // ms de latence simulée
  };

  /* ─── DONNÉES INITIALES ─── */
  let _mentions = [
    { id: 1, nom: 'Informatique',   niveau: 'Master'   },
    { id: 2, nom: 'Mathématiques',  niveau: 'Licence'  },
    { id: 3, nom: 'Commerce',       niveau: 'Licence'  },
    { id: 4, nom: 'Physique',       niveau: 'Doctorat' },
  ];

  let _parcours = [
    { id: 1, mentionId: 1, nom: 'Génie Logiciel',       code: 'GL'  },
    { id: 2, mentionId: 1, nom: 'Systèmes Intelligents', code: 'SI'  },
    { id: 3, mentionId: 2, nom: 'Algèbre & Analyse',     code: 'AA'  },
    { id: 4, mentionId: 3, nom: 'Marketing Digital',     code: 'MD'  },
    { id: 5, mentionId: 4, nom: 'Physique Théorique',    code: 'PT'  },
  ];

  let _groupes = [
    { id: 1, parcoursId: 1, nom: 'Groupe A', effectif: 30 },
    { id: 2, parcoursId: 1, nom: 'Groupe B', effectif: 28 },
    { id: 3, parcoursId: 2, nom: 'Groupe A', effectif: 25 },
    { id: 4, parcoursId: 3, nom: 'Groupe A', effectif: 32 },
    { id: 5, parcoursId: 4, nom: 'Groupe A', effectif: 20 },
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
    {
      id: 3,
      title: 'Réseaux Avancés',
      start: '2025-06-10T08:00:00',
      end:   '2025-06-10T10:00:00',
      salle: 'Salle B204',
      professeur: 'Jean RAKOTO',
      enseignantId: 2,
      groupe: 'Groupe A',
      groupeId: 3,
      type: 'Cours',
      couleur: '#FBBF24',
    },
    {
      id: 4,
      title: 'Algèbre Linéaire',
      start: '2025-06-10T14:00:00',
      end:   '2025-06-10T16:00:00',
      salle: 'Amphi 1',
      professeur: 'Sophie MARTIN',
      enseignantId: 3,
      groupe: 'Groupe A',
      groupeId: 4,
      type: 'TD',
      couleur: '#F87171',
    },
    {
      id: 5,
      title: 'Base de données',
      start: '2025-06-11T08:00:00',
      end:   '2025-06-11T10:00:00',
      salle: 'Labo Info',
      professeur: 'Paul ANDRIA',
      enseignantId: 4,
      groupe: 'Groupe B',
      groupeId: 2,
      type: 'TP',
      couleur: '#A78BFA',
    },
  ];

  let _kpis = {
    totalMentions:   12,
    seancesSemaine:  24,
    tauxOccupation:  78,
    heuresEnseignees: 156,
  };

  let _seancesParJour = {
    labels: ['Lundi', 'Mardi', 'Mercredi', 'Jeudi', 'Vendredi'],
    data:   [8, 10, 7, 9, 5],
  };

  /* ─── HELPERS INTERNES ─── */
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

  function _overlaps(s1Start, s1End, s2Start, s2End) {
    const a = new Date(s1Start).getTime();
    const b = new Date(s1End).getTime();
    const c = new Date(s2Start).getTime();
    const d = new Date(s2End).getTime();
    return a < d && b > c;
  }

  /* ─── PUBLIC API ─── */
  const MockAPI = {

    /* Accès à la config (pour les tests) */
    config,

    /* ── MENTIONS ── */

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
        _kpis.totalMentions++;
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
        const hasLinkedParcours = _parcours.some(p => p.mentionId === id);
        if (hasLinkedParcours) {
          _conflict('Impossible de supprimer : des parcours sont rattachés à cette mention.');
        }
        _mentions.splice(idx, 1);
        if (_kpis.totalMentions > 0) _kpis.totalMentions--;
        return { success: true, id };
      });
    },

    /* ── PARCOURS ── */

    getParcours(mentionId = null) {
      return _simulate(() => {
        let result = JSON.parse(JSON.stringify(_parcours));
        if (mentionId !== null) {
          result = result.filter(p => p.mentionId === mentionId);
        }
        return result;
      });
    },

    getParcoursById(id) {
      return _simulate(() => {
        const p = _parcours.find(x => x.id === id);
        if (!p) _notFound('Parcours', id);
        return JSON.parse(JSON.stringify(p));
      });
    },

    createParcours(data) {
      return _simulate(() => {
        if (!data.nom || !data.nom.trim()) throw { code: 400, message: 'Le nom du parcours est requis.' };
        if (!data.mentionId) throw { code: 400, message: 'La mention est requise.' };
        const mention = _mentions.find(m => m.id === data.mentionId);
        if (!mention) _notFound('Mention', data.mentionId);
        const newParcours = {
          id: _generateId(),
          mentionId: data.mentionId,
          nom: data.nom.trim(),
          code: data.code ? data.code.trim().toUpperCase() : data.nom.trim().substring(0, 2).toUpperCase(),
        };
        _parcours.push(newParcours);
        return JSON.parse(JSON.stringify(newParcours));
      });
    },

    deleteParcours(id) {
      return _simulate(() => {
        const idx = _parcours.findIndex(x => x.id === id);
        if (idx === -1) _notFound('Parcours', id);
        const hasGroupes = _groupes.some(g => g.parcoursId === id);
        if (hasGroupes) _conflict('Des groupes sont rattachés à ce parcours.');
        _parcours.splice(idx, 1);
        return { success: true, id };
      });
    },

    /* ── GROUPES ── */

    getGroupes(parcoursId = null) {
      return _simulate(() => {
        let result = JSON.parse(JSON.stringify(_groupes));
        if (parcoursId !== null) {
          result = result.filter(g => g.parcoursId === parcoursId);
        }
        return result;
      });
    },

    createGroupe(data) {
      return _simulate(() => {
        if (!data.nom || !data.nom.trim()) throw { code: 400, message: 'Le nom du groupe est requis.' };
        if (!data.parcoursId) throw { code: 400, message: 'Le parcours est requis.' };
        const parcours = _parcours.find(p => p.id === data.parcoursId);
        if (!parcours) _notFound('Parcours', data.parcoursId);
        const newGroupe = {
          id: _generateId(),
          parcoursId: data.parcoursId,
          nom: data.nom.trim(),
          effectif: data.effectif ? parseInt(data.effectif, 10) : 0,
        };
        _groupes.push(newGroupe);
        return JSON.parse(JSON.stringify(newGroupe));
      });
    },

    /* ── SÉANCES ── */

    getSeances(filters = {}) {
      return _simulate(() => {
        let result = JSON.parse(JSON.stringify(_seances));
        if (filters.enseignantId) {
          result = result.filter(s => s.enseignantId === filters.enseignantId);
        }
        if (filters.groupeId) {
          result = result.filter(s => s.groupeId === filters.groupeId);
        }
        if (filters.from) {
          const from = new Date(filters.from).getTime();
          result = result.filter(s => new Date(s.start).getTime() >= from);
        }
        if (filters.to) {
          const to = new Date(filters.to).getTime();
          result = result.filter(s => new Date(s.end).getTime() <= to);
        }
        return result;
      });
    },

    getSeanceById(id) {
      return _simulate(() => {
        const s = _seances.find(x => x.id === id);
        if (!s) _notFound('Séance', id);
        return JSON.parse(JSON.stringify(s));
      });
    },

    createSeance(data) {
      return _simulate(() => {
        /* Validation champs requis */
        const required = ['title', 'start', 'end', 'salle', 'enseignantId'];
        for (const field of required) {
          if (!data[field]) throw { code: 400, message: `Le champ "${field}" est requis.` };
        }
        /* Validation cohérence horaire */
        const startMs = new Date(data.start).getTime();
        const endMs   = new Date(data.end).getTime();
        if (endMs <= startMs) throw { code: 400, message: 'L\'heure de fin doit être après l\'heure de début.' };

        /* Vérification conflits via checkConflit */
        const conflicts = _seances.filter(s => {
          const sameEnseignant = s.enseignantId === data.enseignantId;
          const sameSalle      = s.salle === data.salle;
          const sameGroupe     = data.groupeId && s.groupeId === data.groupeId;
          const overlap        = _overlaps(data.start, data.end, s.start, s.end);
          return overlap && (sameEnseignant || sameSalle || sameGroupe);
        });

        if (conflicts.length > 0) {
          const c = conflicts[0];
          _conflict(`Conflit détecté avec "${c.title}" (${c.salle}, ${c.professeur}).`);
        }

        const enseignant = _enseignants.find(e => e.id === data.enseignantId);
        const newSeance = {
          id: _generateId(),
          title: data.title.trim(),
          start: data.start,
          end:   data.end,
          salle: data.salle.trim(),
          professeur: enseignant
            ? `${enseignant.prenom} ${enseignant.nom}`
            : data.professeur || 'Inconnu',
          enseignantId: data.enseignantId,
          groupe:   data.groupe || '',
          groupeId: data.groupeId || null,
          type:     data.type || 'Cours',
          couleur:  data.couleur || '#73B9E6',
        };
        _seances.push(newSeance);
        _kpis.seancesSemaine++;

        /* Recalcul héures enseignées */
        const heures = (new Date(data.end) - new Date(data.start)) / 3600000;
        _kpis.heuresEnseignees = Math.round(_kpis.heuresEnseignees + heures);

        return JSON.parse(JSON.stringify(newSeance));
      });
    },

    updateSeance(id, data) {
      return _simulate(() => {
        const idx = _seances.findIndex(x => x.id === id);
        if (idx === -1) _notFound('Séance', id);

        /* Vérification conflits sans la séance en cours */
        const conflicts = _seances.filter(s => {
          if (s.id === id) return false;
          const sameEnseignant = data.enseignantId && s.enseignantId === data.enseignantId;
          const sameSalle      = data.salle && s.salle === data.salle;
          const sameGroupe     = data.groupeId && s.groupeId === data.groupeId;
          const overlap        = _overlaps(
            data.start || _seances[idx].start,
            data.end   || _seances[idx].end,
            s.start, s.end
          );
          return overlap && (sameEnseignant || sameSalle || sameGroupe);
        });

        if (conflicts.length > 0) {
          const c = conflicts[0];
          _conflict(`Conflit détecté avec "${c.title}" (${c.salle}).`);
        }

        const enseignant = data.enseignantId
          ? _enseignants.find(e => e.id === data.enseignantId)
          : null;

        _seances[idx] = {
          ..._seances[idx],
          ...data,
          professeur: enseignant
            ? `${enseignant.prenom} ${enseignant.nom}`
            : _seances[idx].professeur,
        };
        return JSON.parse(JSON.stringify(_seances[idx]));
      });
    },

    deleteSeance(id) {
      return _simulate(() => {
        const idx = _seances.findIndex(x => x.id === id);
        if (idx === -1) _notFound('Séance', id);
        _seances.splice(idx, 1);
        if (_kpis.seancesSemaine > 0) _kpis.seancesSemaine--;
        return { success: true, id };
      });
    },

    /* ── VÉRIFICATION CONFLIT ─── */
    checkConflit(data) {
      return _simulate(() => {
        const excludeId = data.excludeId || null;
        const conflicts = _seances.filter(s => {
          if (excludeId && s.id === excludeId) return false;
          const overlap        = _overlaps(data.start, data.end, s.start, s.end);
          const sameEnseignant = data.enseignantId && s.enseignantId === data.enseignantId;
          const sameSalle      = data.salle && s.salle === data.salle;
          const sameGroupe     = data.groupeId && s.groupeId === data.groupeId;
          return overlap && (sameEnseignant || sameSalle || sameGroupe);
        });

        return {
          hasConflict: conflicts.length > 0,
          conflicts: conflicts.map(c => ({
            id: c.id,
            title: c.title,
            salle: c.salle,
            professeur: c.professeur,
            start: c.start,
            end: c.end,
          })),
        };
      });
    },

    /* ── ENSEIGNANTS ── */

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

    createEnseignant(data) {
      return _simulate(() => {
        if (!data.nom || !data.nom.trim())    throw { code: 400, message: 'Le nom est requis.' };
        if (!data.prenom || !data.prenom.trim()) throw { code: 400, message: 'Le prénom est requis.' };
        const newEnseignant = {
          id: _generateId(),
          nom:        data.nom.trim().toUpperCase(),
          prenom:     data.prenom.trim(),
          matieres:   Array.isArray(data.matieres) ? data.matieres : [],
          disponible: data.disponible !== false,
        };
        _enseignants.push(newEnseignant);
        return JSON.parse(JSON.stringify(newEnseignant));
      });
    },

    updateEnseignant(id, data) {
      return _simulate(() => {
        const idx = _enseignants.findIndex(x => x.id === id);
        if (idx === -1) _notFound('Enseignant', id);
        _enseignants[idx] = {
          ..._enseignants[idx],
          nom:        data.nom ? data.nom.trim().toUpperCase() : _enseignants[idx].nom,
          prenom:     data.prenom ? data.prenom.trim() : _enseignants[idx].prenom,
          matieres:   Array.isArray(data.matieres) ? data.matieres : _enseignants[idx].matieres,
          disponible: data.disponible !== undefined ? data.disponible : _enseignants[idx].disponible,
        };
        return JSON.parse(JSON.stringify(_enseignants[idx]));
      });
    },

    deleteEnseignant(id) {
      return _simulate(() => {
        const idx = _enseignants.findIndex(x => x.id === id);
        if (idx === -1) _notFound('Enseignant', id);
        const hasSeances = _seances.some(s => s.enseignantId === id);
        if (hasSeances) _conflict('Cet enseignant a des séances programmées. Supprimez-les d\'abord.');
        _enseignants.splice(idx, 1);
        return { success: true, id };
      });
    },

    /* ── KPIs & STATISTIQUES ── */

    getKPIs() {
      return _simulate(() => JSON.parse(JSON.stringify(_kpis)));
    },

    getSeancesParJour() {
      return _simulate(() => {
        /* Recalcul dynamique à partir des séances */
        const jours = ['Lundi', 'Mardi', 'Mercredi', 'Jeudi', 'Vendredi'];
        const dayNums = [1, 2, 3, 4, 5]; /* JS: 0=dim, 1=lun, ... */
        const data = dayNums.map(dayNum =>
          _seances.filter(s => new Date(s.start).getDay() === dayNum).length
        );
        /* Merge avec les données initiales pour avoir un aperçu plus riche */
        const merged = data.map((v, i) => v + (_seancesParJour.data[i] || 0));
        return { labels: jours, data: merged };
      });
    },

    getDisponibilitesEnseignant(enseignantId) {
      return _simulate(() => {
        const seancesEnseignant = _seances.filter(s => s.enseignantId === enseignantId);
        return {
          enseignantId,
          seances: seancesEnseignant.map(s => ({
            start: s.start,
            end:   s.end,
            title: s.title,
          })),
        };
      });
    },
  };

  /* ─── EXPOSITION GLOBALE ─── */
  window.MockAPI = MockAPI;

})();
