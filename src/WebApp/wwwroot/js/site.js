"use strict";
document.addEventListener('DOMContentLoaded', function () {
    prepCheck('#email-check', `${baseUrl}/Check?handler=Email`);
    prepCheck('#database-check', `${baseUrl}/Check?handler=Database`);
    prepCheck('#database-email-check', `${baseUrl}/Check?handler=DatabaseEmail`);
    prepCheck('#service-check', `${baseUrl}/Check?handler=ExternalService`);
    prepCheck('#dotnet-check', `${baseUrl}/Check?handler=DotnetVersion`);

    document.querySelector('#check-all').addEventListener('click', function () {
        document.querySelectorAll('.check-btn').forEach(function (btn) {
            btn.click();
        });
    });
});

function prepCheck(checkSectionId, endPoint) {
    const checkEl = document.querySelector(checkSectionId);
    const buttonEl = checkEl.querySelector('button');
    const resultsEl = checkEl.querySelector('.check-results');

    buttonEl.addEventListener('click', function () {
        resultsEl.innerHTML = '';
        setAsLoading(buttonEl);
        fetch(endPoint)
            .then(response => response.text())
            .then(result => {
                resetAsLoading(buttonEl);
                resultsEl.innerHTML = result;
            })
            .catch(error => {
                resetAsLoading(buttonEl);
                resultsEl.innerHTML = error;
                if (error instanceof Error) {
                    rg4js('send', {error: error, tags: ['handled_promise_rejection']});
                } else {
                    console.error(error);
                }
            });
    });
}

function setAsLoading(btnEl) {
    btnEl.setAttribute('disabled', true);
    btnEl.querySelector('.check-btn-text').classList.add('d-none');
    btnEl.querySelector('.check-btn-spinner').classList.remove('d-none');
    btnEl.querySelector('.check-btn-loading').classList.remove('d-none');
}

function resetAsLoading(btnEl) {
    btnEl.removeAttribute('disabled');
    btnEl.querySelector('.check-btn-text').classList.remove('d-none');
    btnEl.querySelector('.check-btn-spinner').classList.add('d-none');
    btnEl.querySelector('.check-btn-loading').classList.add('d-none');
}
