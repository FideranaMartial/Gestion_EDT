// api.js - Centralise tous les appels API réels

(function () {
    'use strict';

    function getAntiForgeryToken() {
        const token = document.querySelector('input[name="__RequestVerificationToken"]');
        return token ? token.value : '';
    }

    const API = {
        // ── MENTIONS ──────────────────────────────────────────────
        async getMentions() {
            const response = await fetch('/Mentions/GetAll');
            if (!response.ok) {
                const text = await response.text();
                throw new Error('Erreur chargement mentions');
            }
            return response.json();
        },

        async createMention(data) {
            const formData = new FormData();
            formData.append('code_mention', data.code || '');
            formData.append('nom_mention', data.nom);

            const response = await fetch('/Mentions/Create', {
                method: 'POST',
                headers: {
                    'RequestVerificationToken': getAntiForgeryToken()
                },
                body: formData
            });

            if (!response.ok) {
                const text = await response.text();
                throw new Error('Erreur création mention');
            }

            // Redirection gérée par le serveur, on retourne juste un succès
            return { success: true, nom: data.nom };
        },

        async updateMention(id, data) {
            const response = await fetch(`/Mentions/UpdateAjax/${id}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': getAntiForgeryToken()
                },
                body: JSON.stringify({
                    nom: data.nom,
                    code: data.code || '',
                    niveau: data.niveau || ''
                })
            });

            if (!response.ok) {
                throw new Error('Erreur mise à jour');
            }
            return response.json();
        },

        async deleteMention(id) {
            const formData = new FormData();
            formData.append('__RequestVerificationToken', getAntiForgeryToken());

            const response = await fetch(`/Mentions/DeleteAjax/${id}`, {
                method: 'POST',
                headers: {
                    'RequestVerificationToken': getAntiForgeryToken()
                },
                body: formData
            });

            if (!response.ok) {
                throw new Error('Erreur suppression');
            }
            return response.json();
        },

        // ── ENSEIGNANTS ───────────────────────────────────────────
        async getEnseignants() {
            const response = await fetch('/Enseignants/GetAll');
            if (!response.ok) throw new Error('Erreur chargement enseignants');
            return response.json();
        },

        async deleteEnseignant(id) {
            const formData = new FormData();
            formData.append('__RequestVerificationToken', getAntiForgeryToken());

            const response = await fetch(`/Enseignants/DeleteAjax/${id}`, {
                method: 'POST',
                headers: {
                    'RequestVerificationToken': getAntiForgeryToken()
                },
                body: formData
            });
            return response.json();
        },

        // ── SÉANCES ──────────────────────────────────────────────
        async getSeances(filters = {}) {
            let url = '/Seances/GetEvents?';
            if (filters.mentionId) url += `mentionId=${filters.mentionId}&`;
            if (filters.parcoursId) url += `parcoursId=${filters.parcoursId}&`;
            if (filters.enseignantId) url += `enseignantId=${filters.enseignantId}&`;

            const response = await fetch(url);
            if (!response.ok) throw new Error('Erreur chargement séances');
            return response.json();
        },

        async checkConflit(data) {
            const url = `/Seances/CheckConflit?date=${data.date}&heureDebut=${data.heureDebut}&heureFin=${data.heureFin}&salleId=${data.salleId}&enseignantId=${data.enseignantId}`;
            const response = await fetch(url);
            return response.json();
        },

        // ── KPIs ─────────────────────────────────────────────────
        async getKPIs() {
            const response = await fetch('/Home/GetKPIs');
            if (!response.ok) throw new Error('Erreur chargement KPIs');
            return response.json();
        },

        async getSeancesParJour() {
            const response = await fetch('/Home/GetSeancesParJour');
            if (!response.ok) throw new Error('Erreur chargement statistiques');
            return response.json();
        }
    };

    window.API = API;
})();