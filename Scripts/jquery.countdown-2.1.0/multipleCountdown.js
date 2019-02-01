$(document).ready(function () {

});

$(function () {
    var serverTime = document.getElementById("serversidetime").value;
    serverTime = new Date(serverTime);
    var clientTime = new Date();
    var diff = clientTime - serverTime;
    $('[data-countdown]').each(function () {
        var tStr = $(this).data('countdown');
        var target = new Date(tStr);
        var i = new Date(target.getTime() + diff);
        var newTargetDate = ("00" + (i.getMonth() + 1)).slice(-2) + "/" +
            ("00" + i.getDate()).slice(-2) + "/" +
            i.getFullYear() + " " +
            ("00" + i.getHours()).slice(-2) + ":" +
            ("00" + i.getMinutes()).slice(-2) + ":" +
            ("00" + i.getSeconds()).slice(-2);
        $(this).data('countdown', newTargetDate);
        var $this = $(this), finalDate = $(this).data('countdown');
        $this.countdown(finalDate, function (event) {
            $this.html(event.strftime('%D:%H:%M:%S'))
        }).on('finish.countdown', function () {
            setTimeout(function () {
                document.location.reload(true);
                $.ajax({
                    type: 'GET',
                    dataType: 'json',
                    contentType: 'application/json; charset=utf-8',
                    url: '/Auctions/SwitchQueues/' + $this.attr('id'),
                    success: function (result) {
                        if (result.Status == "CO") {

                        };
                    }
                });
            }, 2500);
        });
    });
});
