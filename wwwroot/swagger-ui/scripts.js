(function () {
    const storageKey = 'swagger_request_body';
    const textareaSelector = 'textarea.body-param__text';
    const executeButtonSelector = '.btn.execute.opblock-control__btn';

    function saveBody() {
        const textarea = document.querySelector(textareaSelector);
        if (textarea) {
            console.log('Saving request body to localStorage:', textarea.value);
            localStorage.setItem(storageKey, textarea.value);
        } else {
            console.log('No textarea found to save request body.');
        }
    }

    function restoreBody() {
        const saved = localStorage.getItem(storageKey);
        const textarea = document.querySelector(textareaSelector);
        if (textarea && saved) {
            console.log('Restoring request body from localStorage:', saved);
            textarea.value = saved;
            textarea.dispatchEvent(new Event('input', { bubbles: true }));
        } else {
            console.log('No saved request body found or no textarea available to restore.');
        }
    }

    function attachRestoreToExecuteButtons() {
        var b = $('.btn.try-out__btn').on('click', () => {
            button.addEventListener('click', () => {
                setTimeout(restoreBody, 100); // чуть позже, чтобы DOM успел обновиться  
            });

        });


        document.querySelectorAll(executeButtonSelector).forEach(button => {
            console.log('Attaching restoreBody to execute button:', button);
            button.addEventListener('click', () => {
                setTimeout(restoreBody, 100); // чуть позже, чтобы DOM успел обновиться  
            });
        });
    }

    function attachSaveToExecuteButton() {
        const observer = new MutationObserver(() => {
            var executeBtn = document.querySelector('.execute-wrapper .btn[type="button"]');
            if (executeBtn) {
                console.log('Attaching saveBody to execute button:', executeBtn);
                executeBtn.addEventListener('click', saveBody);
                observer.disconnect();
            } else {
                console.log('Execute button not found yet, waiting...');
            }
        });

        observer.observe(document.body, {
            childList: true,
            subtree: true
        });
    }

    if (document.readyState === 'loading') {
        console.log('Document is loading, attaching event listeners on DOMContentLoaded.');
        document.addEventListener('DOMContentLoaded', () => {
            attachRestoreToExecuteButtons();
            attachSaveToExecuteButton();
            restoreBody();
        });
    } else {
        console.log('Document already loaded, attaching event listeners immediately.');
        attachRestoreToExecuteButtons();
        attachSaveToExecuteButton();
    }
})();
