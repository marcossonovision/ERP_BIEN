document.addEventListener("DOMContentLoaded", () => {
    const manage = document.querySelectorAll('#gridManage .dash-card').length;
    const actions = document.querySelectorAll('#gridActions .dash-card').length;
    const my = document.querySelectorAll('#gridMy .dash-card').length;

    const total = manage + actions + my;

    const elTotal = document.getElementById('kpiModules');
    const elManage = document.getElementById('kpiManage');
    const elMy = document.getElementById('kpiMy');

    if (elTotal) elTotal.textContent = total;
    if (elManage) elManage.textContent = manage;
    if (elMy) elMy.textContent = my;
});