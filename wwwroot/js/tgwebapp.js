document.addEventListener('DOMContentLoaded', function () {
    let touchStartX = 0;
    let touchStartY = 0;
    let touchEndX = 0;
    let touchEndY = 0;

    document.addEventListener('touchstart', function (event) {
        touchStartX = event.changedTouches[0].clientX;
        touchStartY = event.changedTouches[0].clientY;
    }, false);

    document.addEventListener('touchend', function (event) {
        touchEndX = event.changedTouches[0].clientX;
        touchEndY = event.changedTouches[0].clientY;

        const deltaX = touchEndX - touchStartX;
        const deltaY = Math.abs(touchEndY - touchStartY);

        const screenHeight = window.innerHeight;
        const screenWidth = window.innerWidth;

        const centerY = screenHeight / 2;
        const touchY = (touchStartY + touchEndY) / 2;
        const isInCenterY = Math.abs(touchY - centerY) < screenHeight / 4;

        const isInLeftZone = touchStartX < screenWidth * 0.4;

        if (deltaX > 100 && deltaY < 75 && isInCenterY && isInLeftZone) {
            window.history.back();
        }
    }, false);
});

function goBack() {
    window.history.back();
}

function onExportingHandler(e) {
    const defaultName = '@defaultExportFileName';
    DevExpress.ui.dialog.custom({
        title: "Сохранить как",
        messageHtml: "<div>Введите имя файла:</div><input type='text' id='filenameInput' value='" + defaultName + "' style='width:100%' />",
        buttons: [
            {
                text: "Сохранить", onClick: () => {
                    const filename = document.getElementById('filenameInput').value || defaultName;
                    e.fileName = filename;

                    if (e.format === 'xlsx') {

                        var workbook = new ExcelJS.Workbook();
                        var worksheet = workbook.addWorksheet('Main sheet');
                        DevExpress.excelExporter.exportDataGrid({
                            worksheet: worksheet,
                            component: e.component,
                            customizeCell: function (options) {
                                options.excelCell.font = { name: 'Arial', size: 12 };
                                options.excelCell.alignment = { horizontal: 'left' };
                            }
                        }).then(function () {
                            workbook.xlsx.writeBuffer().then(function (buffer) {
                                saveAs(new Blob([buffer], { type: 'application/octet-stream' }), `${e.fileName}.xlsx`);
                            });
                        });


                    }
                    else if (e.format === 'pdf') {
                        const doc = new jspdf.jsPDF();
                        DevExpress.pdfExporter.exportDataGrid({
                            jsPDFDocument: doc,
                            component: e.component,
                        }).then(() => {
                            doc.save(`${e.fileName}.pdf`);
                        });

                    }




                }
            },
            { text: "Отмена", onClick: () => e.cancel = true }
        ]
    }).show();

    // отменяем экспорт по умолчанию, он продолжится в диалоге
    e.cancel = true;
}

