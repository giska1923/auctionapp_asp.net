$(function () {
    // Reference the auto-generated proxy for the hub.  
    var chat = $.connection.displayHub;
    // Create a function that the hub can call back to display messages.
    chat.client.addNewMessageToPage = function (currentP, currency, id) {
        $('.displayobjectJson').each(function () {
            var $elem = $(this);
            if (id == $elem.attr('value')) {
                var s = '';
                s = '<p class="bg-warning text-success">'
                    + currentP + ' ' + currency + '</p>';

                $elem.html(s);
                setTimeout(function () {
                    var s = '';
                    s = '' + currentP + ' ' + currency;
                    $elem.html(s);
                }, 2500);
            }
        });
    };

    chat.client.addNewItemToTable = function (email, tokensBidded, timestamp) {
        $('.displayBiddedUser').each(function () {
            var $elem = $(this);
            var old = $elem.html();
            
            var s = '<tr><td>' + email + '</td><td class="getTokenClass">' + tokensBidded + '</td><td>' + timestamp + '</td></tr>';
            s = s + old;
            $elem.html(s);
            $('#tokensNow').val(tokensBidded);
        });
    };
    // Start the connection.
    $.connection.hub.start().done(function () {
        $('#insertBid').click(function () {
            // Call the Send method on the hub.
            var submited = $('#submitValue').val();
            var tokensNow = $('#tokensNow').val();
            if (parseInt(submited) > parseInt(tokensNow)) {
                chat.server.send($('#submitValue').val(), $('#submitCurrency').val(), $('#currencyValue').val(), $('#id').val());
                chat.server.sendForTable($('#myEmail').val(), $('#submitValue').val());
                $('#tokensNow').val(tokensBidded);
            }
        });
    });
});