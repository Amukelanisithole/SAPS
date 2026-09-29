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
        try { localStorage.setItem('saps-theme', theme); } catch (e) { }
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

(function () {
    document.addEventListener('submit', function (event) {
        var form = event.target.closest('form[data-confirm]');
        if (!form || form.dataset.confirmed === 'true') return;
        if (typeof Swal === 'undefined') return;

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
                form.requestSubmit();
            }
        });
    });
})();

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

(function () {
    if (typeof $ === 'undefined' || !$.fn.select2) return;

    $(function () {
        $('select:not([multiple])').not('[data-no-select2]').each(function () {
            $(this).select2({ width: '100%' });
        });
    });
})();

(function () {
    var LEVELS = [
        { label: '', color: 'transparent', width: 0 },
        { label: 'Very weak', color: '#dc3545', width: 20 },
        { label: 'Weak', color: '#f0883e', width: 40 },
        { label: 'Fair', color: '#d9a441', width: 60 },
        { label: 'Good', color: '#2f66b3', width: 80 },
        { label: 'Strong', color: '#16a34a', width: 100 }
    ];

    function scorePassword(value) {
        if (!value) return 0;
        var score = 0;
        if (value.length >= 8) score++;
        if (/[A-Z]/.test(value)) score++;
        if (/[a-z]/.test(value)) score++;
        if (/[0-9]/.test(value)) score++;
        if (/[^a-zA-Z0-9]/.test(value)) score++;
        return score;
    }

    document.addEventListener('DOMContentLoaded', function () {
        document.querySelectorAll('input[data-strength-meter]').forEach(function (input) {
            var group = input.closest('.saps-field-group');
            if (!group) return;

            var wrap = document.createElement('div');
            wrap.className = 'saps-password-strength';
            wrap.innerHTML = '<div class="saps-password-strength-track"><div class="saps-password-strength-fill"></div></div>' +
                '<span class="saps-password-strength-label"></span>';
            group.insertAdjacentElement('afterend', wrap);

            var fill = wrap.querySelector('.saps-password-strength-fill');
            var label = wrap.querySelector('.saps-password-strength-label');

            input.addEventListener('input', function () {
                var level = LEVELS[scorePassword(input.value)];
                fill.style.width = level.width + '%';
                fill.style.backgroundColor = level.color;
                label.textContent = input.value ? level.label : '';
                label.style.color = level.color === 'transparent' ? '' : level.color;
            });
        });
    });
})();

(function () {
    var reduceMotion = window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches;

    document.addEventListener('DOMContentLoaded', function () {
        document.querySelectorAll('.saps-small-box-value').forEach(function (el) {
            var target = parseInt(el.textContent.trim(), 10);
            if (isNaN(target)) return;

            if (reduceMotion) {
                el.textContent = target;
                return;
            }

            var duration = 800;
            var start = null;
            el.textContent = '0';

            function step(timestamp) {
                if (start === null) start = timestamp;
                var progress = Math.min((timestamp - start) / duration, 1);
                var eased = 1 - Math.pow(1 - progress, 3);
                el.textContent = Math.round(eased * target);
                if (progress < 1) requestAnimationFrame(step);
            }
            requestAnimationFrame(step);
        });
    });
})();

(function () {
    document.addEventListener('click', function (event) {
        var btn = event.target.closest('.btn-primary, .btn-warning, .btn-danger');
        if (!btn) return;

        var rect = btn.getBoundingClientRect();
        var size = Math.max(rect.width, rect.height);
        var ripple = document.createElement('span');
        ripple.className = 'saps-btn-ripple';
        ripple.style.width = ripple.style.height = size + 'px';
        ripple.style.left = (event.clientX - rect.left - size / 2) + 'px';
        ripple.style.top = (event.clientY - rect.top - size / 2) + 'px';
        btn.appendChild(ripple);
        ripple.addEventListener('animationend', function () { ripple.remove(); });
    });
})();

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
        try { localStorage.setItem('saps-sidebar-collapsed', collapsed ? 'true' : 'false'); } catch (e) { }
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

    document.addEventListener('submit', function (event) {
        var form = event.target;
        if (!(form instanceof HTMLFormElement) || form.hasAttribute('data-no-spinner')) return;

        setTimeout(function () {
            if (!event.defaultPrevented) showSpinner();
        }, 0);
    });

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

    window.addEventListener('pageshow', function (event) {
        if (event.persisted) hideSpinner();
    });

    document.addEventListener('DOMContentLoaded', hideSpinner);
})();
