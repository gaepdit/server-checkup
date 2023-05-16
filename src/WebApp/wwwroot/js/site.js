$(document).ready(function () {
    prepCheck('#email-check', baseUrl + '/Check?handler=Email');
    prepCheck('#database-check', baseUrl + '/Check?handler=Database');
    prepCheck('#service-check', baseUrl + '/Check?handler=ExternalService');
    prepCheck('#dotnet-check', baseUrl + '/Check?handler=DotnetVersion');

    $('#check-all').click(function () {
        $('.check-btn').click();
    });
});

function prepCheck(checkSectionId, endPoint) {
    const checkEl = $(checkSectionId);
    const buttonEl = checkEl.find('button').first();
    const resultsEl = checkEl.find('.check-results').first();

    buttonEl.click(function () {
        resultsEl.empty();
        setAsLoading(buttonEl);
        $.ajax({
            type: "Get",
            url: endPoint,
            success: function (result) {
                resetAsLoading(buttonEl);
                resultsEl.html(result);
            },
            error(msg) {
                resetAsLoading(buttonEl);
                resultsEl.html(msg);
            }
        });
    });
}

function setAsLoading(btnEl) {
    btnEl.prop('disabled', true);
    btnEl.find('.check-btn-text').first().addClass('d-none');
    btnEl.find('.check-btn-spinner').first().removeClass('d-none');
    btnEl.find('.check-btn-loading').first().removeClass('d-none');
}

function resetAsLoading(btnEl) {
    btnEl.prop('disabled', false);
    btnEl.find('.check-btn-text').first().removeClass('d-none');
    btnEl.find('.check-btn-spinner').first().addClass('d-none');
    btnEl.find('.check-btn-loading').first().addClass('d-none');
}
