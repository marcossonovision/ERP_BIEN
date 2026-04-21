document.addEventListener('DOMContentLoaded', () => {

    const modal = document.getElementById('licenseModal');

    const producto = document.getElementById('modalProducto');
    const code = document.getElementById('modalCode');
    const proveedor = document.getElementById('modalProveedor');
    const precio = document.getElementById('modalPrecio');
    const caducidad = document.getElementById('modalCaducidad');
    const image = document.getElementById('modalImage');

    document.querySelectorAll('.license-card').forEach(card => {
        card.addEventListener('click', () => {
            producto.textContent = card.dataset.producto;
            code.textContent = card.dataset.code;
            proveedor.textContent = card.dataset.proveedor;
            precio.textContent = card.dataset.precio;
            caducidad.textContent = card.dataset.caducidad;
            image.src = `/img/licenses/${card.dataset.image}.png`;

            modal.classList.add('show');
        });
    });

    modal.querySelector('.modal-close').addEventListener('click', () => {
        modal.classList.remove('show');
    });

    modal.addEventListener('click', e => {
        if (e.target === modal) {
            modal.classList.remove('show');
        }
    });
});