// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.


// Write your JavaScript code.
function cambiarContrasena() {
    const actual = document.getElementById("contrasenaActual").value.trim();
    const nueva = document.getElementById("contrasenaNueva").value.trim();
    const mensaje = document.getElementById("mensaje");


    if (!actual || !nueva) {
        mensaje.style.color = "red";
        mensaje.textContent = "Por favor complete ambos campos.";
        return;
    }


    fetch("/Usuario/CambiarContrasena", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
            contrasenaActual: actual,
            contrasenaNueva: nueva
        })
    })
    .then(r => r.json())
    .then(data => {
        if (data.bloqueado) {
            mensaje.style.color = "red";
            mensaje.textContent = "Se acabaron los intentos. Inténtelo más tarde.";
        } else if (data.success) {
            mensaje.style.color = "green";
            mensaje.textContent = "Contraseña cambiada correctamente.";
            document.getElementById("formCambio").reset();
        } else {
            mensaje.style.color = "red";
            mensaje.textContent = "Contraseña incorrecta. Intento fallido.";
        }
    })
    .catch(() => {
        mensaje.style.color = "red";
        mensaje.textContent = "Error al conectar con el servidor.";
    });
}


(function () {
    const diasContainer = document.querySelector("[data-calendario-dias]");
    const detallePanel = document.querySelector("[data-calendario-detalle]");
    const detalleFecha = document.querySelector(".Detalle-Fecha");
    const detalleTitulo = document.querySelector(".Detalle-Titulo");
    const detalleDireccion = document.querySelector("[data-detalle-direccion]");
    const detalleHora = document.querySelector("[data-detalle-hora]");
    const detalleDescripcion = document.querySelector("[data-detalle-descripcion]");
    const detalleCerrar = document.querySelector(".Detalle-Cerrar");
    const detalleInfo = document.querySelector(".Detalle-Info");
    const mesActual = document.querySelector(".Calendario-MesActual");


    if (!diasContainer || !mesActual) return;


    const estadoCalendario = {
        fecha: new Date(),
        eventos: []
    };


    function normalizarEventosIniciales() {
        if (!Array.isArray(window.calendarioEventos)) {
            estadoCalendario.eventos = [];
            return;
        }

        estadoCalendario.eventos = window.calendarioEventos
            .map(ev => {
                if (!ev.fecha) return null;
                const fechaEvento = new Date(ev.fecha);
                if (Number.isNaN(fechaEvento.getTime())) return null;
                
                return {
                    id: ev.id,
                    fecha: fechaEvento,
                    nombreMedico: ev.nombreMedico || "",
                    apellidoMedico: ev.apellidoMedico || "",
                    nombreOrganizacion: ev.nombreOrganizacion || "",
                    direccion: ev.direccion || "",
                    descripcion: ev.descripcion || ""
                };
            })
            .filter(ev => ev !== null);
    }


    function formatearMes(fecha) {
        return fecha.toLocaleDateString("es-AR", { month: "long", year: "numeric" });
    }


    function crearDiaElemento(diaNumero, esActual, tieneEvento, fecha) {
        const btn = document.createElement("button");
        btn.className = "Calendario-Dia";
        btn.type = "button";
        btn.textContent = diaNumero;
        btn.dataset.fecha = fecha.toISOString();


        if (esActual) btn.classList.add("activo");
        if (tieneEvento) {
            btn.classList.add("con-evento");
            const indicador = document.createElement("span");
            indicador.className = "Indicador-Evento";
            btn.appendChild(indicador);
        }


        btn.addEventListener("click", () => mostrarDetalle(fecha));
        return btn;
    }


    function renderizarCalendario() {
        diasContainer.innerHTML = "";
        const fecha = estadoCalendario.fecha;
        const primerDiaSemana = (new Date(fecha.getFullYear(), fecha.getMonth(), 1).getDay() + 6) % 7; // ajuste para lunes
        const diasMes = new Date(fecha.getFullYear(), fecha.getMonth() + 1, 0).getDate();


        mesActual.textContent = formatearMes(fecha).replace(/^\w/, c => c.toUpperCase());


        // Dias vacíos previos
        for (let i = 0; i < primerDiaSemana; i++) {
            const placeholder = document.createElement("div");
            placeholder.className = "Calendario-Dia placeholder";
            diasContainer.appendChild(placeholder);
        }


        const hoy = new Date();
        hoy.setHours(0, 0, 0, 0);
        
        for (let dia = 1; dia <= diasMes; dia++) {
            const fechaDia = new Date(fecha.getFullYear(), fecha.getMonth(), dia);
            fechaDia.setHours(0, 0, 0, 0);
            
            const tieneEvento = estadoCalendario.eventos.some(ev => {
                const fechaEvento = new Date(ev.fecha);
                fechaEvento.setHours(0, 0, 0, 0);
                return fechaEvento.getTime() === fechaDia.getTime();
            });
            
            const esActual = fechaDia.getTime() === hoy.getTime();
            diasContainer.appendChild(crearDiaElemento(dia, esActual, tieneEvento, fechaDia));
        }
    }


    function mostrarDetalle(fecha) {
        const fechaComparar = new Date(fecha);
        fechaComparar.setHours(0, 0, 0, 0);
        
        const eventosDia = estadoCalendario.eventos.filter(ev => {
            const fechaEvento = new Date(ev.fecha);
            fechaEvento.setHours(0, 0, 0, 0);
            return fechaEvento.getTime() === fechaComparar.getTime();
        });

        if (!eventosDia.length) {
            if (detallePanel) {
                detallePanel.hidden = false;
                if (detalleFecha) {
                    detalleFecha.textContent = fecha.toLocaleDateString("es-AR", { 
                        day: "numeric", 
                        month: "long", 
                        year: "numeric" 
                    });
                }
                if (detalleTitulo) {
                    detalleTitulo.textContent = "No hay eventos programados para este día";
                }
                // Ocultar la sección de información cuando no hay eventos
                if (detalleInfo) {
                    detalleInfo.style.display = "none";
                }
            }
            return;
        }

        const evento = eventosDia[0];
        const fechaEvento = new Date(evento.fecha);
        
        // Mostrar la sección de información cuando hay eventos
        if (detalleInfo) {
            detalleInfo.style.display = "flex";
        }
        
        if (detalleFecha) {
            detalleFecha.textContent = fecha.toLocaleDateString("es-AR", { 
                day: "numeric", 
                month: "long", 
                year: "numeric" 
            });
        }
        if (detalleTitulo) {
            detalleTitulo.textContent = "Turno medico";
        }
        if (detalleDireccion) {
            detalleDireccion.textContent = evento.direccion || "No especificada";
        }
        if (detalleHora) {
            detalleHora.textContent = fechaEvento.toLocaleTimeString("es-AR", { 
                hour: "2-digit", 
                minute: "2-digit" 
            }) + "hs";
        }
        if (detalleDescripcion) {
            detalleDescripcion.textContent = evento.descripcion || "Sin descripción";
        }

        if (detallePanel) {
            detallePanel.hidden = false;
        }
    }


    detalleCerrar?.addEventListener("click", () => {
        detallePanel.hidden = true;
    });


    document.querySelector(".Cal-BtnPrev")?.addEventListener("click", () => {
        estadoCalendario.fecha.setMonth(estadoCalendario.fecha.getMonth() - 1);
        renderizarCalendario();
        detallePanel.hidden = true;
    });


    document.querySelector(".Cal-BtnNext")?.addEventListener("click", () => {
        estadoCalendario.fecha.setMonth(estadoCalendario.fecha.getMonth() + 1);
        renderizarCalendario();
        detallePanel.hidden = true;
    });
    
    


    normalizarEventosIniciales();
    renderizarCalendario();
})();

// Funciones para editar perfil
let campoActual = '';
let valorActual = '';

function abrirModal(campo, valor) {
    campoActual = campo;
    valorActual = valor;
    
    const modal = document.getElementById('modal-editar');
    if (!modal) {
        console.error('Modal no encontrado');
        return;
    }
    
    const campoNombre = document.getElementById('modal-campo-nombre');
    const valorActualSpan = document.getElementById('modal-valor-actual');
    const inputTexto = document.getElementById('modal-nuevo-valor');
    const inputFecha = document.getElementById('modal-nuevo-valor-date');
    
    // Configurar nombres amigables
    const nombresCampos = {
        'Email': 'Email',
        'Nombre': 'Nombre',
        'Apellido': 'Apellido',
        'Doc_nro': 'Número de Documento',
        'FechaNacimiento': 'Fecha de Nacimiento',
        'Sexo': 'Sexo',
        'PesoEnKg': 'Peso',
        'AlturaEnCm': 'Altura',
        'Telefono': 'Teléfono'
    };
    
    if (campoNombre) campoNombre.textContent = nombresCampos[campo] || campo;
    if (valorActualSpan) valorActualSpan.textContent = valor;
    
    // Mostrar el input apropiado según el tipo de campo
    if (campo === 'FechaNacimiento') {
        if (inputTexto) inputTexto.style.display = 'none';
        if (inputFecha) {
            inputFecha.style.display = 'block';
            inputFecha.value = valor;
        }
    } else {
        if (inputTexto) {
            inputTexto.style.display = 'block';
            inputTexto.value = valor;
        }
        if (inputFecha) inputFecha.style.display = 'none';
    }
    
    modal.style.display = 'flex';
}

function cerrarModal() {
    document.getElementById('modal-editar').style.display = 'none';
    campoActual = '';
    valorActual = '';
}

function guardarCambio() {
    const inputTexto = document.getElementById('modal-nuevo-valor');
    const inputFecha = document.getElementById('modal-nuevo-valor-date');
    const nuevoValor = campoActual === 'FechaNacimiento' ? inputFecha.value : inputTexto.value;
    
    if (!nuevoValor || nuevoValor.trim() === '') {
        alert('Por favor, ingrese un valor válido.');
        return;
    }
    
    // Enviar la petición al servidor
    fetch('/Home/ActualizarCampoUsuario', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({
            Campo: campoActual,
            Valor: nuevoValor
        })
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            // Actualizar el valor en la vista
            const elementoValor = document.getElementById('valor-' + campoActual);
            if (elementoValor) {
                if (campoActual === 'FechaNacimiento') {
                    const fecha = new Date(nuevoValor);
                    elementoValor.textContent = fecha.toLocaleDateString('es-ES');
                } else {
                    elementoValor.textContent = nuevoValor;
                }
            }
            cerrarModal();
            alert('Campo actualizado correctamente.');
            // Recargar la página para mostrar los cambios
            location.reload();
        } else {
            alert('Error al actualizar el campo: ' + (data.message || 'Error desconocido'));
        }
    })
    .catch(error => {
        console.error('Error:', error);
        alert('Ocurrió un error al actualizar el campo.');
    });
}

// Cerrar modal al hacer clic fuera de él
window.addEventListener('click', function(event) {
    const modal = document.getElementById('modal-editar');
    if (event.target === modal) {
        cerrarModal();
    }
});

// Funciones para medicamentos
function toggleIndicaciones(elemento) {
    const card = elemento.closest('.medicamento-card');
    const detalle = card.querySelector('.medicamento-indicaciones-detalle');
    const indicaciones = card.querySelector('.medicamento-indicaciones');
    
    if (detalle.style.display === 'none' || detalle.style.display === '') {
        detalle.style.display = 'block';
        indicaciones.classList.add('active');
    } else {
        detalle.style.display = 'none';
        indicaciones.classList.remove('active');
    }
}
// Modal editar medicamento
function abrirModalMedicamento(id, nombre, dosis, frecuencia, hora, indicacion) {
    const modal = document.getElementById('modal-medicamento');
    if (!modal) {
        console.error('Modal de medicamento no encontrado');
        return;
    }
    
    document.getElementById('med-id').value = id;
    document.getElementById('med-nombre').value = nombre || '';
    document.getElementById('med-dosis').value = dosis || '';
    document.getElementById('med-frecuencia').value = frecuencia || '';
    document.getElementById('med-hora').value = hora || '';
    document.getElementById('med-indicacion').value = indicacion || '';
    
    modal.style.display = 'flex';
}

function cerrarModalMedicamento() {
    const modal = document.getElementById('modal-medicamento');
    if (modal) {
        modal.style.display = 'none';
    }
}

function cambiarFormulario() {
    const valor = document.getElementById("tipoDato").value;

    document.querySelectorAll(".formulario-manual").forEach(form => {
        form.classList.add("oculto");
    });

    if (valor === "vacunacion") {
        document.getElementById("formVacunacion").classList.remove("oculto");
    }
    if (valor === "estudio") {
        document.getElementById("formEstudio").classList.remove("oculto");
    }
    if (valor === "enfermedad") {
        document.getElementById("formEnfermedad").classList.remove("oculto");
    }
    if (valor === "antecedente") {
        document.getElementById("formAntecedente").classList.remove("oculto");
    }
    if (valor === "consulta") {
        document.getElementById("formConsulta").classList.remove("oculto");
    }
}

function guardarMedicamento() {
    const id = document.getElementById('med-id').value;
    const datos = {
        Id: parseInt(id),
        Nombre_Comercial: document.getElementById('med-nombre').value,
        Dosis: document.getElementById('med-dosis').value,
        Frecuencia: document.getElementById('med-frecuencia').value,
        HoraProgramada: document.getElementById('med-hora').value,
        Indicacion: document.getElementById('med-indicacion').value
    };
    
    if (!datos.Nombre_Comercial || !datos.Dosis) {
        alert('Por favor complete los campos obligatorios (Nombre y Dosis).');
        return;
    }
    
    fetch('/Home/ActualizarMedicamento', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(datos)
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            cerrarModalMedicamento();
            alert('Medicamento actualizado correctamente.');
            location.reload();
        } else {
            alert('Error al actualizar: ' + (data.message || 'Error desconocido'));
        }
    })
    .catch(error => {
        console.error('Error:', error);
        alert('Ocurrió un error al actualizar el medicamento.');
    });
}

function confirmarEliminarMedicamento(id) {
    if (confirm('¿Está seguro que desea eliminar este medicamento?')) {
        fetch('/Home/EliminarMedicamento', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ Id: id })
        })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                alert('Medicamento eliminado correctamente.');
                location.reload();
            } else {
                alert('Error al eliminar: ' + (data.message || 'Error desconocido'));
            }
        })
        .catch(error => {
            console.error('Error:', error);
            alert('Ocurrió un error al eliminar el medicamento.');
        });
    }
}

function tomarMedicamento(id) {
    const btn = document.getElementById('btn-tome-' + id);
    const cantidadElement = document.getElementById('cantidad-' + id);
    const card = btn.closest('.medicamento-card');
    
    // Deshabilitar el botón mientras se procesa
    btn.disabled = true;
    btn.textContent = 'Procesando...';
    
    fetch('/Home/TomarMedicamento', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({ Id: id })
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            // Actualizar la cantidad en la vista
            if (cantidadElement) {
                cantidadElement.textContent = data.cantidad;
                
                // Si no hay más pastillas, deshabilitar el botón y cambiar el estilo del card
                if (data.cantidad <= 0) {
                    btn.disabled = true;
                    btn.textContent = 'Sin pastillas';
                    btn.style.opacity = '0.5';
                    // Agregar clase para el estilo rojo
                    if (card) {
                        card.classList.add('sin-pastillas');
                    }
                } else {
                    btn.disabled = false;
                    btn.textContent = 'Tomé';
                    // Remover clase si había llegado a 0 antes
                    if (card) {
                        card.classList.remove('sin-pastillas');
                    }
                }
            }
        } else {
            alert('Error: ' + (data.message || 'No se pudo registrar la toma'));
            btn.disabled = false;
            btn.textContent = 'Tomé';
        }
    })
    .catch(error => {
        console.error('Error:', error);
        alert('Ocurrió un error al registrar la toma del medicamento.');
        btn.disabled = false;
        btn.textContent = 'Tomé';
    });
}

// Cerrar modal de medicamento al hacer clic fuera
window.addEventListener('click', function(event) {
    const modalMed = document.getElementById('modal-medicamento');
    if (event.target === modalMed) {
        cerrarModalMedicamento();
    }
});

// ==================== FUNCIONES PARA HISTORIAL MÉDICO ====================

function verMasHistorial(button) {
    const tipo = button.getAttribute('data-tipo');
    
    if (tipo === 'documento') {
        mostrarDetallesDocumento(button);
    } else if (tipo === 'vacuna') {
        mostrarDetallesVacuna(button);
    } else if (tipo === 'enfermedad' || tipo === 'diagnostico') {
        mostrarDetallesDiagnostico(button, tipo);
    }
}

function mostrarDetallesDocumento(button) {
    const nombre = button.getAttribute('data-nombre');
    const titulo = button.getAttribute('data-titulo') || nombre;
    const fecha = button.getAttribute('data-fecha');
    const tipoArchivo = button.getAttribute('data-tipoarchivo');
    const idArchivo = button.getAttribute('data-idarchivo');
    const id = button.getAttribute('data-id');
    
    document.getElementById('modal-titulo').textContent = 'Detalles del Documento';
    
    let contenido = `
        <div class="detalle-item">
            <strong>Título:</strong> <span>${titulo || 'Sin título'}</span>
        </div>
        <div class="detalle-item">
            <strong>Nombre del archivo:</strong> <span>${nombre || 'Sin nombre'}</span>
        </div>
        <div class="detalle-item">
            <strong>Fecha:</strong> <span>${fecha || 'No especificada'}</span>
        </div>
        <div class="detalle-item">
            <strong>Tipo de archivo:</strong> <span>${tipoArchivo || 'No especificado'}</span>
        </div>
    `;
    
    document.getElementById('modal-body').innerHTML = contenido;
    
    // Si es PDF, mostrar botones de descarga y vista previa
    let footer = '';
    if (tipoArchivo && tipoArchivo.toLowerCase().includes('pdf')) {
        footer = `
            <button class="btn-descargar-pdf" onclick="descargarPDF(${id}, ${idArchivo || 'null'})">Descargar PDF</button>
            <button class="btn-ver-pdf" onclick="verPDF(${id}, ${idArchivo || 'null'})">Ver PDF</button>
        `;
    }
    document.getElementById('modal-footer').innerHTML = footer;
    
    document.getElementById('modal-detalles').style.display = 'flex';
}

function mostrarDetallesVacuna(button) {
    const nombre = button.getAttribute('data-nombre');
    const dosis = button.getAttribute('data-dosis');
    const aplicacion = button.getAttribute('data-aplicacion');
    const fecha = button.getAttribute('data-fecha');
    
    document.getElementById('modal-titulo').textContent = 'Detalles de la Vacunación';
    
    let contenido = `
        <div class="detalle-item">
            <strong>Nombre de la vacuna:</strong> <span>${nombre || 'No especificado'}</span>
        </div>
        <div class="detalle-item">
            <strong>Dosis:</strong> <span>${dosis || 'No especificada'}</span>
        </div>
        <div class="detalle-item">
            <strong>Aplicación:</strong> <span>${aplicacion || 'No especificada'}</span>
        </div>
        <div class="detalle-item">
            <strong>Fecha de aplicación:</strong> <span>${fecha || 'No especificada'}</span>
        </div>
    `;
    
    document.getElementById('modal-body').innerHTML = contenido;
    document.getElementById('modal-footer').innerHTML = '';
    document.getElementById('modal-detalles').style.display = 'flex';
}

function mostrarDetallesDiagnostico(button, tipo) {
    const nombre = button.getAttribute('data-nombre');
    const descripcion = button.getAttribute('data-descripcion');
    const fechaInicio = button.getAttribute('data-fechainicio');
    const fechaFin = button.getAttribute('data-fechafin');
    const estado = button.getAttribute('data-estado') === 'True' ? 'Activo' : 'Inactivo';
    
    const tituloTipo = tipo === 'enfermedad' ? 'Enfermedad' : 'Diagnóstico';
    document.getElementById('modal-titulo').textContent = `Detalles de la ${tituloTipo}`;
    
    let contenido = `
        <div class="detalle-item">
            <strong>Nombre:</strong> <span>${nombre || 'No especificado'}</span>
        </div>
        <div class="detalle-item">
            <strong>Descripción:</strong> <span>${descripcion || 'Sin descripción'}</span>
        </div>
        <div class="detalle-item">
            <strong>Fecha de inicio:</strong> <span>${fechaInicio || 'No especificada'}</span>
        </div>
        ${fechaFin ? `<div class="detalle-item">
            <strong>Fecha de fin:</strong> <span>${fechaFin}</span>
        </div>` : ''}
        <div class="detalle-item">
            <strong>Estado:</strong> <span>${estado}</span>
        </div>
    `;
    
    document.getElementById('modal-body').innerHTML = contenido;
    document.getElementById('modal-footer').innerHTML = '';
    document.getElementById('modal-detalles').style.display = 'flex';
}

function cerrarModalDetalles() {
    document.getElementById('modal-detalles').style.display = 'none';
}

function descargarPDF(idDocumento, idArchivo) {
    if (!idArchivo || idArchivo === 'null') {
        alert('No hay archivo disponible para descargar');
        return;
    }
    window.location.href = `/Home/DescargarDocumento?id=${idDocumento}&idArchivo=${idArchivo}`;
}

function verPDF(idDocumento, idArchivo) {
    if (!idArchivo || idArchivo === 'null') {
        alert('No hay archivo disponible para visualizar');
        return;
    }
    window.open(`/Home/VerDocumento?id=${idDocumento}&idArchivo=${idArchivo}`, '_blank');
}

// Cerrar modal al hacer clic fuera
document.addEventListener('DOMContentLoaded', function() {
    const modalDetalles = document.getElementById('modal-detalles');
    if (modalDetalles) {
        modalDetalles.addEventListener('click', function(e) {
            if (e.target === this) {
                cerrarModalDetalles();
            }
        });
    }
});


