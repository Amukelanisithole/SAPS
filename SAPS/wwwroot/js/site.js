// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Fades in any ".reveal" element the first time it scrolls into view. Skipped
// entirely when the browser doesn't support IntersectionObserver or the user
// has asked for reduced motion — elements are visible by default either way,
// this only adds the entrance animation on top.
(function () {
    if (!('IntersectionObserver' in window)) return;
    if (window.matchMedia('(prefers-reduced-motion: reduce)').matches) return;

    var observer = new IntersectionObserver(function (entries) {
        entries.forEach(function (entry) {
            if (entry.isIntersecting) {
                entry.target.classList.add('is-visible');
                observer.unobserve(entry.target);
            }
        });
    }, { threshold: 0.15 });

    document.querySelectorAll('.reveal').forEach(function (el) {
        observer.observe(el);
    });
})();

// Light/dark theme toggle. The initial attribute is set synchronously by an
// inline script in <head> (before this file loads) to avoid a flash of the
// wrong theme — this just wires up the toggle button(s) and keeps every
// button's icon/label in sync, since a page can have one in both the public
// navbar and the app topbar layouts (never both at once, but the code
// doesn't assume which).
(function () {
    function currentTheme() {
        return document.documentElement.getAttribute('data-bs-theme') === 'light' ? 'light' : 'dark';
    }

    function applyIcon(theme) {
        document.querySelectorAll('[data-theme-toggle] i').forEach(function (icon) {
            icon.className = theme === 'dark' ? 'bi bi-sun' : 'bi bi-moon-stars';
        });
    }

    function setTheme(theme) {
        document.documentElement.setAttribute('data-bs-theme', theme);
        try { localStorage.setItem('saps-theme', theme); } catch (e) { /* private browsing, etc. */ }
        applyIcon(theme);
    }

    document.addEventListener('DOMContentLoaded', function () {
        applyIcon(currentTheme());

        document.querySelectorAll('[data-theme-toggle]').forEach(function (button) {
            button.addEventListener('click', function () {
                setTheme(currentTheme() === 'dark' ? 'light' : 'dark');
            });
        });
    });
})();

// Fires a SweetAlert2 toast for the TempData success/error message the
// current page was rendered with — set as window.sapsFlash by a small inline
// script in _Layout/_DashboardLayout so every controller's TempData response
// gets the same swal treatment without each view wiring it up itself.
(function () {
    document.addEventListener('DOMContentLoaded', function () {
        if (typeof Swal === 'undefined' || !window.sapsFlash) return;

        var toast = Swal.mixin({
            toast: true,
            position: 'top-end',
            showConfirmButton: false,
            timer: 4500,
            timerProgressBar: true
        });

        if (window.sapsFlash.success) {
            toast.fire({ icon: 'success', title: window.sapsFlash.success });
        }
        if (window.sapsFlash.error) {
            toast.fire({ icon: 'error', title: window.sapsFlash.error });
        }
    });
})();

// Swal-backed replacement for native confirm() on destructive form
// submissions. Any <form data-confirm="Question?"> is intercepted on submit
// and only actually submitted once the user accepts the SweetAlert2 prompt —
// add data-confirm-text for the body copy and data-confirm-button for the
// confirm button's label, both optional.
(function () {
    document.addEventListener('submit', function (event) {
        var form = event.target.closest('form[data-confirm]');
        if (!form || form.dataset.confirmed === 'true') return;
        if (typeof Swal === 'undefined') return; // fall back to a plain submit if swal failed to load

        event.preventDefault();
        Swal.fire({
            title: form.dataset.confirm,
            text: form.dataset.confirmText || 'This action cannot be undone.',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: form.dataset.confirmButton || 'Yes',
            cancelButtonText: 'Cancel',
            confirmButtonColor: '#dc3545',
            reverseButtons: true
        }).then(function (result) {
            if (result.isConfirmed) {
                form.dataset.confirmed = 'true';
                // requestSubmit() (rather than submit()) still fires a real
                // submit event, so the global spinner below still picks this
                // up even though it's a programmatic, not user, submission.
                form.requestSubmit();
            }
        });
    });
})();

// Show/hide toggle for password fields. Delegated to the document (rather
// than bound per-button) so it works on every page — including ones with no
// other page-specific script block — without each view needing its own
// wiring code.
(function () {
    document.addEventListener('click', function (event) {
        var button = event.target.closest('.saps-toggle-password');
        if (!button) return;

        var input = document.getElementById(button.getAttribute('data-target'));
        if (!input) return;

        var showing = input.type === 'text';
        input.type = showing ? 'password' : 'text';

        var icon = button.querySelector('i');
        if (icon) {
            icon.className = showing ? 'bi bi-eye' : 'bi bi-eye-slash';
        }
        button.setAttribute('aria-label', showing ? 'Show password' : 'Hide password');
    });
})();

// Strips any non-digit characters as the user types into a field marked
// data-digits-only, so ID number / phone number inputs can't be typed (or
// pasted) past their required digit count in the first place — the server
// side IdNumberAttribute/PhoneNumberDigitsAttribute stay the source of truth,
// this is just the browser-side complement so a mistyped extra digit never
// gets that far. Delegated to the document like the other page-wide widgets
// above, so it works on any input with the attribute without per-view wiring.
(function () {
    document.addEventListener('input', function (event) {
        var input = event.target;
        if (!input.matches || !input.matches('[data-digits-only]')) return;

        var digitsOnly = input.value.replace(/\D/g, '');
        var maxLength = input.getAttribute('maxlength');
        if (maxLength) {
            digitsOnly = digitsOnly.slice(0, parseInt(maxLength, 10));
        }
        if (digitsOnly !== input.value) {
            input.value = digitsOnly;
        }
    });
})();

// Select2 for every single-select dropdown in the dashboard app (jQuery/
// select2 are only loaded on _DashboardLayout, so this is a no-op — and thus
// harmless — on the public site). Opt out per-select with data-no-select2.
// Select2 always renders its dropdown panel into <body>, outside whatever
// container the original <select> lived in, but its theming still works
// because [data-bs-theme] is set on <html>, an ancestor of that panel either
// way — see the .select2-* rules in site.css for both themes.
(function () {
    if (typeof $ === 'undefined' || !$.fn.select2) return;

    $(function () {
        $('select:not([multiple])').not('[data-no-select2]').each(function () {
            $(this).select2({ width: '100%' });
        });
    });
})();

// Dashboard sidebar collapse toggle (desktop only — the initial state is set
// synchronously by an inline script in _DashboardLayout's <head> to avoid a
// flash of the wrong width; this just wires up the button and keeps its
// tooltip/aria-label in sync).
(function () {
    function isCollapsed() {
        return document.documentElement.classList.contains('saps-sidebar-collapsed');
    }

    function applyLabel(collapsed) {
        document.querySelectorAll('[data-sidebar-toggle]').forEach(function (button) {
            var label = collapsed ? 'Expand sidebar' : 'Collapse sidebar';
            button.setAttribute('aria-label', label);
            button.setAttribute('title', label);
        });
    }

    function setCollapsed(collapsed) {
        document.documentElement.classList.toggle('saps-sidebar-collapsed', collapsed);
        try { localStorage.setItem('saps-sidebar-collapsed', collapsed ? 'true' : 'false'); } catch (e) { /* private browsing, etc. */ }
        applyLabel(collapsed);
    }

    document.addEventListener('DOMContentLoaded', function () {
        applyLabel(isCollapsed());

        document.querySelectorAll('[data-sidebar-toggle]').forEach(function (button) {
            button.addEventListener('click', function () {
                setCollapsed(!isCollapsed());
            });
        });
    });
})();

// Full-view "Processing…" overlay for any real form submission or page
// navigation, so a multi-second round trip (an affidavit's photo/PDF
// upload, geocoding lookups, email sends, etc.) never looks like the page
// just froze. Delegated at the document level so it covers every page
// without each view wiring it up, and opt-out via a data-no-spinner
// attribute on a specific form/link for the rare case that needs it.
(function () {
    var HIDE_FAILSAFE_MS = 20000;
    var failsafeTimer = null;

    function overlay() {
        return document.getElementById('sapsGlobalSpinner');
    }

    function showSpinner() {
        var el = overlay();
        if (!el) return;
        el.classList.add('show');
        el.setAttribute('aria-hidden', 'false');
        // Don't leave a visitor staring at a blocked page forever if
        // something this logic treated as a real navigation never actually
        // navigates away (a popup blocked by the browser, a link to a
        // fragment this couldn't recognize, etc.).
        clearTimeout(failsafeTimer);
        failsafeTimer = setTimeout(hideSpinner, HIDE_FAILSAFE_MS);
    }

    function hideSpinner() {
        var el = overlay();
        if (el) {
            el.classList.remove('show');
            el.setAttribute('aria-hidden', 'true');
        }
        clearTimeout(failsafeTimer);
    }

    // Forms: jQuery's unobtrusive validation runs its own submit handler on
    // this same event and calls preventDefault() when the form is invalid —
    // deferring the check to a fresh task lets that handler run first, so
    // defaultPrevented actually reflects whether the submission proceeded.
    document.addEventListener('submit', function (event) {
        var form = event.target;
        if (!(form instanceof HTMLFormElement) || form.hasAttribute('data-no-spinner')) return;

        setTimeout(function () {
            if (!event.defaultPrevented) showSpinner();
        }, 0);
    });

    // Links: only ones that actually cause this page to navigate away —
    // excludes new-tab/download links, in-page anchors, tel:/mailto:, and
    // Bootstrap's own dropdown/modal/collapse/offcanvas triggers (which use
    // <a> but never navigate anywhere).
    document.addEventListener('click', function (event) {
        if (event.defaultPrevented || event.button !== 0 || event.ctrlKey || event.metaKey || event.shiftKey || event.altKey) return;

        var link = event.target.closest('a[href]');
        if (!link || link.hasAttribute('data-no-spinner')) return;
        if (link.target && link.target !== '_self') return;
        if (link.hasAttribute('download')) return;
        if (link.hasAttribute('data-bs-toggle') || link.hasAttribute('data-bs-dismiss')) return;

        var href = link.getAttribute('href');
        if (!href || href.charAt(0) === '#' || /^(javascript|mailto|tel):/i.test(href)) return;

        showSpinner();
    });

    // A page restored from the back/forward cache never re-fires
    // DOMContentLoaded, so a spinner left showing right before the visitor
    // navigated away would otherwise still be showing when they come back.
    window.addEventListener('pageshow', function (event) {
        if (event.persisted) hideSpinner();
    });

    document.addEventListener('DOMContentLoaded', hideSpinner);
})();
