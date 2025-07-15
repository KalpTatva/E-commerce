$(document).ready(function() {
    function getCookie(name) {
        const value = `; ${document.cookie}`;
        const parts = value.split(`; ${name}=`);
        return parts.length === 2 ? parts.pop().split(';').shift() : undefined;
    }

    function updateCookie(name, value) {
        document.cookie = `${name}=${value};path=/;max-age=3600`;
    }

    function setTheme(theme) {
        if (theme === 'system') {
            $('html').removeAttr('data-theme');
        } else {
            $('html').attr('data-theme', theme);
        }

        const index = themes.indexOf(theme);
        $('.theme-icon')
            .removeClass('bi-circle-half bi-sun bi-moon')
            .addClass(icons[index])
            .attr('data-theme', theme)
            .attr('title', titles[index]);

        $.ajax({
            url: '/Dashboard/ThemeChange',
            type: 'POST',
            data: { theme: theme },
            success: function (data) {
                if (!data.success) {
                    toastr.error(data.message, "Error", { timeOut: 4000 });
                }
            },
            error: function () {
                toastr.error('Failed to change theme in db.', "Error", { timeOut: 4000 });
            }
        });

        updateCookie('theme', theme);
    }

    const themes = ['system', 'light', 'dark'];
    const icons = ['bi-circle-half', 'bi-sun', 'bi-moon'];
    const titles = ['System Theme', 'Light Theme', 'Dark Theme'];

    // Initialize theme
    let currentTheme = getCookie('theme') || 'system';
    setTheme(currentTheme);

    $('.theme-toggle').on('click', '.theme-icon', function () {
        const currentIndex = themes.indexOf($(this).attr('data-theme'));
        const nextTheme = themes[(currentIndex + 1) % themes.length];
        setTheme(nextTheme);
    });

    window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', (e) => {
        if (getCookie('theme') === 'system') {
            console.log('System theme changed to:', e.matches ? 'dark' : 'light');
            setTheme('system');
        }
    });
});