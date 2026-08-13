/* Welcome card - live clock + time-of-day greeting.
   Reads the user's name from the #greetingMessage element's data-name. */

(function () {
    "use strict";

    var $greeting = $('#greetingMessage');
    if (!$greeting.length) return;

    var userName = $greeting.data('name') || '';

    function updateDateTime() {
        var now = new Date();
        var hours = now.getHours();
        var minutes = now.getMinutes().toString().padStart(2, '0');
        var seconds = now.getSeconds().toString().padStart(2, '0');
        var ampm = hours >= 12 ? 'PM' : 'AM';
        var h12 = hours % 12 || 12;

        var day = now.getDate().toString().padStart(2, '0');
        var monthNames = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
        var month = monthNames[now.getMonth()];
        var year = now.getFullYear();

        var formattedDateTime = day + '-' + month + '-' + year + ' ' + h12 + ':' + minutes + ':' + seconds + ' ' + ampm;

        var greeting = hours < 12 ? 'Good Morning' : (hours < 18 ? 'Good Afternoon' : 'Good Evening');

        document.getElementById('greetingMessage').innerText = greeting + ', ' + userName + '!';
        document.getElementById('dateTime').innerText = 'Today, ' + formattedDateTime;
    }

    setInterval(updateDateTime, 1000);
    updateDateTime();
})();
