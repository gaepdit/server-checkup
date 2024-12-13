"use strict";
let connection;

document.addEventListener('DOMContentLoaded', function () {
    connection = new signalR.HubConnectionBuilder().withUrl(`${baseUrl}/checkHub`).build();
    connection.on("ReceiveCheckResult", (checkId, text, details, context) =>
        handleCheckResult(checkId, text, details, context));
    connection.start();

    prepCheck('email-check', `${baseUrl}/Check?handler=Email`);
    prepCheck('database-check', `${baseUrl}/Check?handler=Database`);
    prepCheck('database-email-check', `${baseUrl}/Check?handler=DatabaseEmail`);
    prepCheck('service-check', `${baseUrl}/Check?handler=ExternalService`);
    prepCheck('dotnet-check', `${baseUrl}/Check?handler=DotnetVersion`);

    document.querySelector('#check-all').addEventListener('click', function () {
        document.querySelectorAll('.check-btn').forEach(function (btn) {
            btn.click();
        });
    });
});

function handleCheckResult(checkId, text, details, context) {
    const checkEl = document.querySelector(`#${checkId}`);
    const resultsEl = checkEl.querySelector('.list-group');

    const messageEl = document.createElement('li');
    messageEl.className = `list-group-item list-group-item-${getContextClass(context)}`;

    const divEl = document.createElement('div');
    divEl.className = 'd-flex align-items-center';

    const svgNS = 'http://www.w3.org/2000/svg';
    const svgEl = document.createElementNS(svgNS, 'svg');
    svgEl.setAttribute('class', 'bi flex-shrink-0 me-2');
    svgEl.setAttribute('role', 'img');
    svgEl.setAttribute('aria-label', `${context}:`);

    const useEl = document.createElementNS(svgNS, 'use');
    useEl.setAttribute('href', `#${context}`);
    svgEl.appendChild(useEl);

    const textDivEl = document.createElement('div');
    textDivEl.textContent = text;

    if (details) {
        const detailsDivEl = document.createElement('div');
        detailsDivEl.className = 'text-muted';
        detailsDivEl.textContent = details;
        textDivEl.appendChild(detailsDivEl);
    }

    divEl.appendChild(svgEl);
    divEl.appendChild(textDivEl);
    messageEl.appendChild(divEl);
    resultsEl.appendChild(messageEl);
}

function getContextClass(context) {
    const contextClasses = {
        'Success': 'success',
        'Error': 'danger',
        'Warning': 'warning'
    };
    return contextClasses[context] || 'info';
}

function prepCheck(checkSectionId, endPoint) {
    const checkEl = document.querySelector(`#${checkSectionId}`);
    const buttonEl = checkEl.querySelector('button');
    const resultsEl = checkEl.querySelector('.check-results');

    buttonEl.addEventListener('click', function () {
        resultsEl.innerHTML = '';
        const messageListEl = document.createElement('ul');
        messageListEl.className = 'list-group';
        resultsEl.appendChild(messageListEl);

        setAsLoading(buttonEl);
        fetch(endPoint)
            .then(response => response.text())
            .then(result => {
                resetAsLoading(buttonEl);
                resultsEl.innerHTML = result;
            })
            .catch(error => {
                resetAsLoading(buttonEl);
                handleCheckResult(checkSectionId, 'An error occurred', error, 'Error');
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
