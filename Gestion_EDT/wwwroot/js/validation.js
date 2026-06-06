// ═══════════════════════════════════════
// FICHIER : /wwwroot/js/validation.js
// Application : EMIT — Gestion Emploi du Temps
// ═══════════════════════════════════════

(function () {
  'use strict';

  /* ════════════════════════════════════════
     RÈGLES DE VALIDATION
     ════════════════════════════════════════ */

  const RULES = {

    /**
     * required — Champ non vide
     */
    required: {
      validate: (value) => value.trim().length > 0,
      message:  'Ce champ est obligatoire.',
    },

    /**
     * alpha — Lettres, espaces, tirets, apostrophes uniquement
     * Adapté aux noms propres francophones
     */
    alpha: {
      validate: (value) => /^[A-Za-zÀ-ÖØ-öø-ÿ\s'\-]+$/.test(value.trim()),
      message:  'Ce champ ne doit contenir que des lettres.',
    },

    /**
     * alphanumeric — Lettres, chiffres, espaces
     */
    alphanumeric: {
      validate: (value) => /^[A-Za-zÀ-ÖØ-öø-ÿ0-9\s'\-]+$/.test(value.trim()),
      message:  'Ce champ ne doit contenir que des lettres et des chiffres.',
    },

    /**
     * tel — Numéro de téléphone (formats FR/MG et international)
     * Accepte : +261 32 00 000 00 | 032 00 000 00 | +33 6 12 34 56 78 | etc.
     */
    tel: {
      validate: (value) => {
        const cleaned = value.replace(/[\s\-\.\(\)]/g, '');
        return /^(\+?\d{7,15})$/.test(cleaned);
      },
      message: 'Numéro de téléphone invalide (ex: +261 32 00 000 00).',
    },

    /**
     * email — Adresse email valide
     */
    email: {
      validate: (value) => /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value.trim()),
      message:  'Adresse email invalide.',
    },

    /**
     * capacity — Nombre entier positif (effectif d'un groupe)
     * Plage autorisée : 1 à 1000
     */
    capacity: {
      validate: (value) => {
        const n = parseInt(value, 10);
        return !isNaN(n) && n >= 1 && n <= 1000 && String(n) === value.trim();
      },
      message: 'L\'effectif doit être un nombre entier entre 1 et 1 000.',
    },

    /**
     * positive-int — Nombre entier positif générique
     */
    'positive-int': {
      validate: (value) => {
        const n = parseInt(value, 10);
        return !isNaN(n) && n > 0;
      },
      message: 'Ce champ doit être un nombre entier positif.',
    },

    /**
     * min-length — Longueur minimale (configuré via data-min-length)
     */
    'min-length': {
      validate: (value, el) => {
        const min = parseInt(el.dataset.minLength || '2', 10);
        return value.trim().length >= min;
      },
      message: (el) => {
        const min = el.dataset.minLength || '2';
        return `Ce champ doit contenir au moins ${min} caractères.`;
      },
    },

    /**
     * max-length — Longueur maximale (configuré via data-max-length)
     */
    'max-length': {
      validate: (value, el) => {
        const max = parseInt(el.dataset.maxLength || '255', 10);
        return value.trim().length <= max;
      },
      message: (el) => {
        const max = el.dataset.maxLength || '255';
        return `Ce champ ne peut pas dépasser ${max} caractères.`;
      },
    },

    /**
     * time-range — Valide qu'une heure de fin est après l'heure de début
     * Usage : data-validate="time-range" data-time-start="#startInput"
     */
    'time-range': {
      validate: (value, el) => {
        const startSelector = el.dataset.timeStart;
        if (!startSelector) return true;
        const startEl = document.querySelector(startSelector);
        if (!startEl || !startEl.value) return true;
        const toMinutes = (t) => {
          const [h, m] = t.split(':').map(Number);
          return h * 60 + (m || 0);
        };
        return toMinutes(value) > toMinutes(startEl.value);
      },
      message: 'L\'heure de fin doit être après l\'heure de début.',
    },

    /**
     * datetime-end — Valide qu'un datetime-local de fin est après le début
     * Usage : data-validate="datetime-end" data-datetime-start="#startDatetimeInput"
     */
    'datetime-end': {
      validate: (value, el) => {
        const startSelector = el.dataset.datetimeStart;
        if (!startSelector) return true;
        const startEl = document.querySelector(startSelector);
        if (!startEl || !startEl.value) return true;
        return new Date(value).getTime() > new Date(startEl.value).getTime();
      },
      message: 'La date/heure de fin doit être après la date/heure de début.',
    },

    /**
     * salle — Code de salle (alphanumérique, 2-20 chars)
     */
    salle: {
      validate: (value) => /^[A-Za-z0-9\s\-\.]{2,20}$/.test(value.trim()),
      message:  'Code de salle invalide (2-20 caractères alphanumériques).',
    },

    /**
     * code — Code court en majuscules (2-6 caractères)
     */
    code: {
      validate: (value) => /^[A-Z0-9]{2,6}$/.test(value.trim().toUpperCase()),
      message:  'Le code doit contenir 2 à 6 caractères majuscules.',
    },
  };


  /* ════════════════════════════════════════
     MOTEUR DE VALIDATION
     ════════════════════════════════════════ */

  const ValidationEngine = {

    rules: RULES,

    /**
     * Valide un seul champ.
     * @param {HTMLElement} el - L'input/select/textarea
     * @returns {{ valid: boolean, message: string }}
     */
    validateField(el) {
      const rawRules   = (el.dataset.validate || '').trim();
      const value      = el.value || '';
      const isRequired = el.hasAttribute('required') || rawRules.includes('required');

      /* Si le champ est vide et non requis, on le remet neutre */
      if (!value.trim() && !isRequired) {
        this._setNeutral(el);
        return { valid: true, message: '' };
      }

      /* Vérification required d'abord */
      if (isRequired && !value.trim()) {
        this._setInvalid(el, RULES.required.message);
        return { valid: false, message: RULES.required.message };
      }

      /* Parsing des règles multiples séparées par '|' ou espace */
      const ruleNames = rawRules
        .split(/[\|\s]+/)
        .map(r => r.trim())
        .filter(r => r && r !== 'required');

      for (const ruleName of ruleNames) {
        const rule = RULES[ruleName];
        if (!rule) {
          console.warn(`[EMIT Validation] Règle inconnue : "${ruleName}"`);
          continue;
        }
        const isValid = rule.validate(value, el);
        if (!isValid) {
          const msg = typeof rule.message === 'function'
            ? rule.message(el)
            : rule.message;
          this._setInvalid(el, msg);
          return { valid: false, message: msg };
        }
      }

      /* Toutes les règles passées */
      this._setValid(el);
      return { valid: true, message: '' };
    },

    /**
     * Valide tous les champs d'un formulaire.
     * @param {HTMLFormElement|HTMLElement} form - Le formulaire ou conteneur
     * @returns {{ valid: boolean, firstInvalidEl: HTMLElement|null, errors: string[] }}
     */
    validateForm(form) {
      const fields = form.querySelectorAll('[data-validate], [required]');
      let valid          = true;
      let firstInvalidEl = null;
      const errors       = [];

      fields.forEach(el => {
        /* Skip les éléments désactivés ou cachés */
        if (el.disabled || el.closest('[hidden]') || el.closest('.d-none')) return;

        const result = this.validateField(el);
        if (!result.valid) {
          valid = false;
          errors.push(result.message);
          if (!firstInvalidEl) firstInvalidEl = el;
        }
      });

      /* Scroll vers le premier champ invalide */
      if (firstInvalidEl) {
        firstInvalidEl.scrollIntoView({ behavior: 'smooth', block: 'center' });
        firstInvalidEl.focus();
      }

      return { valid, firstInvalidEl, errors };
    },

    /**
     * Réinitialise tous les états visuels d'un formulaire.
     * @param {HTMLFormElement|HTMLElement} form
     */
    resetForm(form) {
      const fields = form.querySelectorAll('.form-control-emit, .form-select-emit');
      fields.forEach(el => this._setNeutral(el));
    },

    /**
     * Ajoute une règle personnalisée.
     * @param {string}   name     - Identifiant de la règle
     * @param {Function} validate - (value, el) => boolean
     * @param {string}   message  - Message d'erreur
     */
    addRule(name, validate, message) {
      if (RULES[name]) {
        console.warn(`[EMIT Validation] La règle "${name}" existe déjà, elle sera écrasée.`);
      }
      RULES[name] = { validate, message };
    },


    /* ─── Feedback visuel ─── */

    _setValid(el) {
      el.classList.remove('is-invalid');
      el.classList.add('is-valid');
      const feedback = this._getFeedback(el);
      if (feedback) {
        feedback.className = 'form-feedback-emit valid-feedback';
        feedback.textContent = '';
      }
    },

    _setInvalid(el, message) {
      el.classList.remove('is-valid');
      el.classList.add('is-invalid');
      const feedback = this._getFeedback(el);
      if (feedback) {
        feedback.className = 'form-feedback-emit invalid-feedback';
        feedback.textContent = message;
      }
    },

    _setNeutral(el) {
      el.classList.remove('is-valid', 'is-invalid');
      const feedback = this._getFeedback(el);
      if (feedback) {
        feedback.className = 'form-feedback-emit';
        feedback.textContent = '';
      }
    },

    /**
     * Trouve l'élément de feedback associé à un champ.
     * Cherche en priorité :
     * 1. Un élément [data-feedback-for="#id"]
     * 2. Le sibling .form-feedback-emit direct
     * 3. L'élément .form-feedback-emit dans le parent .form-group-emit
     */
    _getFeedback(el) {
      if (el.id) {
        const byAttr = document.querySelector(`[data-feedback-for="#${el.id}"]`);
        if (byAttr) return byAttr;
      }

      /* Sibling direct */
      let sibling = el.nextElementSibling;
      while (sibling) {
        if (sibling.classList.contains('form-feedback-emit')) return sibling;
        sibling = sibling.nextElementSibling;
      }

      /* Parent form-group-emit */
      const group = el.closest('.form-group-emit');
      if (group) {
        return group.querySelector('.form-feedback-emit');
      }

      return null;
    },
  };


  /* ════════════════════════════════════════
     LISTENERS AUTOMATIQUES
     ════════════════════════════════════════ */

  function attachAutoListeners() {
    /* On attache sur tous les champs avec data-validate ou required */
    const selector = 'input[data-validate], select[data-validate], textarea[data-validate], input[required], select[required], textarea[required]';

    function attachToEl(el) {
      if (el._emitValidationAttached) return;
      el._emitValidationAttached = true;

      /* Validation à la perte de focus */
      el.addEventListener('blur', () => {
        /* Ne valide que si le champ a déjà été touché ou a une valeur */
        if (el._emitTouched || el.value.trim()) {
          ValidationEngine.validateField(el);
        }
      });

      /* Marquer comme touché à la première interaction */
      el.addEventListener('input', () => {
        el._emitTouched = true;
        /* Si déjà en erreur, re-valide en live pour feedback positif immédiat */
        if (el.classList.contains('is-invalid')) {
          ValidationEngine.validateField(el);
        }
      });

      el.addEventListener('change', () => {
        el._emitTouched = true;
        ValidationEngine.validateField(el);
      });

      /* Pour les champs de type time-range / datetime-end :
         re-valider le champ de fin quand le début change */
      if (el.id) {
        const dependents = document.querySelectorAll(
          `[data-time-start="#${el.id}"], [data-datetime-start="#${el.id}"]`
        );
        dependents.forEach(dep => {
          el.addEventListener('change', () => {
            if (dep._emitTouched || dep.value.trim()) {
              ValidationEngine.validateField(dep);
            }
          });
        });
      }
    }

    /* Attache aux éléments existants */
    document.querySelectorAll(selector).forEach(attachToEl);

    /* MutationObserver pour les champs ajoutés dynamiquement (modales, etc.) */
    const observer = new MutationObserver((mutations) => {
      mutations.forEach(mutation => {
        mutation.addedNodes.forEach(node => {
          if (node.nodeType !== 1) return; /* Pas un élément */
          if (node.matches && node.matches(selector)) {
            attachToEl(node);
          }
          /* Descendants */
          node.querySelectorAll && node.querySelectorAll(selector).forEach(attachToEl);
        });
      });
    });

    observer.observe(document.body, { childList: true, subtree: true });
  }


  /* ════════════════════════════════════════
     INITIALISATION
     ════════════════════════════════════════ */

  document.addEventListener('DOMContentLoaded', () => {
    attachAutoListeners();

    /* Intercept les formulaires avec data-emit-validate pour empêcher la
       soumission HTML native et valider avant tout fetch() */
    document.querySelectorAll('form[data-emit-validate]').forEach(form => {
      form.addEventListener('submit', (e) => {
        e.preventDefault();
        const result = ValidationEngine.validateForm(form);
        if (!result.valid) {
          if (window.showToast) {
            window.showToast(
              `${result.errors.length} erreur(s) dans le formulaire. Vérifiez les champs en rouge.`,
              'warning',
              'Formulaire invalide'
            );
          }
        }
        /* Si valide, on laisse le JS de la vue gérer la soumission via MockAPI */
        const event = new CustomEvent('emit:form-validated', {
          detail: result,
          bubbles: true,
        });
        form.dispatchEvent(event);
      });
    });
  });


  /* ════════════════════════════════════════
     EXPOSITION GLOBALE
     ════════════════════════════════════════ */

  window.ValidationEngine = ValidationEngine;

})();
