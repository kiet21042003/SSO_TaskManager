// Biến để lưu trữ task đang được kéo
let draggedTask = null;

// Xử lý sự kiện bắt đầu kéo
function dragStart(event) {
    draggedTask = event.target;
    event.target.classList.add('dragging');
}

// Xử lý sự kiện kết thúc kéo
function dragEnd(event) {
    event.target.classList.remove('dragging');
    draggedTask = null;
    
    // Xóa hiệu ứng drag-over khỏi tất cả các cột
    document.querySelectorAll('.task-column').forEach(column => {
        column.classList.remove('drag-over');
    });
}

// Xử lý sự kiện kéo qua
function dragOver(event) {
    event.preventDefault();
    const column = event.target.closest('.task-column');
    if (column) {
        column.classList.add('drag-over');
    }
}

// Xử lý sự kiện rời khỏi vùng thả
function dragLeave(event) {
    const column = event.target.closest('.task-column');
    if (column) {
        column.classList.remove('drag-over');
    }
}

// Xử lý sự kiện thả
async function drop(event) {
    event.preventDefault();
    const column = event.target.closest('.task-column');
    if (!column || !draggedTask) return;

    const newStatus = parseInt(column.dataset.status);
    const taskId = parseInt(draggedTask.id.replace('task-', ''));
    
    try {
        await moveTask(taskId, newStatus);
    } catch (error) {
        console.error('Lỗi khi di chuyển task:', error);
        showToast('Có lỗi xảy ra khi di chuyển công việc', 'error');
    }
}

// Hàm để cập nhật trạng thái công việc
async function moveTask(id, newStatus) {
    try {
        const response = await fetch('/api/tasks/move', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            },
            body: JSON.stringify({
                taskId: id,
                newStatus: newStatus
            })
        });

        if (!response.ok) {
            const data = await response.json();
            throw new Error(data.message || 'Có lỗi xảy ra khi cập nhật trạng thái công việc');
        }

        showToast('Cập nhật trạng thái thành công', 'success');
        setTimeout(() => window.location.reload(), 1000);
    } catch (error) {
        console.error('Error:', error);
        showToast(error.message, 'error');
    }
}

// Hàm hiển thị thông báo
function showToast(message, type = 'success') {
    Toastify({
        text: message,
        duration: 3000,
        close: true,
        gravity: "top",
        position: "right",
        style: {
            background: type === 'success' ? "#198754" : "#dc3545"
        }
    }).showToast();
}

// Hàm để chỉnh sửa task
function editTask(id, title, description) {
    document.getElementById('editTaskId').value = id;
    document.getElementById('editTaskTitle').value = title;
    document.getElementById('editTaskDescription').value = description;
    
    const modal = new bootstrap.Modal(document.getElementById('editTaskModal'));
    modal.show();
}

// Hàm để xóa task
async function deleteTask(id) {
    if (!confirm('Bạn có chắc chắn muốn xóa công việc này?')) return;

    try {
        const response = await fetch(`/api/tasks/${id}`, {
            method: 'DELETE',
            headers: {
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            }
        });

        if (!response.ok) {
            const data = await response.json();
            throw new Error(data.message || 'Có lỗi xảy ra khi xóa công việc');
        }

        showToast('Xóa công việc thành công');
        setTimeout(() => window.location.reload(), 1000);
    } catch (error) {
        console.error('Error:', error);
        showToast(error.message, 'error');
    }
}

// Thêm các event listener cho các cột task
document.addEventListener('DOMContentLoaded', () => {
    const taskColumns = document.querySelectorAll('.task-column');
    taskColumns.forEach(column => {
        column.addEventListener('dragover', dragOver);
        column.addEventListener('dragleave', dragLeave);
        column.addEventListener('drop', drop);
    });

    // Reset form khi đóng modal
    const addTaskModal = document.getElementById('addTaskModal');
    if (addTaskModal) {
        addTaskModal.addEventListener('hidden.bs.modal', () => {
            const form = document.getElementById('taskForm');
            if (form) {
                form.reset();
                const titleInput = document.getElementById('taskTitle');
                titleInput.classList.remove('is-invalid');
            }
        });
    }

    // Khởi tạo các event listener khi trang được load
    const taskForm = document.getElementById('taskForm');
    if (taskForm) {
        taskForm.addEventListener('submit', createTask);
    }

    // Form chỉnh sửa
    const editTaskForm = document.getElementById('editTaskForm');
    if (editTaskForm) {
        editTaskForm.addEventListener('submit', updateTask);
    }
});

// Hàm để tạo task mới
async function createTask(event) {
    event.preventDefault();
    
    const title = document.getElementById('taskTitle').value.trim();
    const description = document.getElementById('taskDescription').value.trim();
    const status = parseInt(document.getElementById('taskStatus').value);

    if (!title) {
        document.getElementById('taskTitle').classList.add('is-invalid');
        return;
    }

    try {
        const response = await fetch('/api/tasks', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            },
            body: JSON.stringify({
                title,
                description,
                status
            })
        });

        if (!response.ok) {
            const data = await response.json();
            throw new Error(data.message || 'Có lỗi xảy ra khi tạo công việc');
        }

        showToast('Tạo công việc thành công');
        const modal = bootstrap.Modal.getInstance(document.getElementById('addTaskModal'));
        modal.hide();
        setTimeout(() => window.location.reload(), 1000);
    } catch (error) {
        console.error('Error:', error);
        showToast(error.message, 'error');
    }
}

// Hàm để cập nhật task
async function updateTask(event) {
    event.preventDefault();
    
    const id = document.getElementById('editTaskId').value;
    const title = document.getElementById('editTaskTitle').value.trim();
    const description = document.getElementById('editTaskDescription').value.trim();

    if (!title) {
        document.getElementById('editTaskTitle').classList.add('is-invalid');
        return;
    }

    try {
        const response = await fetch(`/api/tasks/${id}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            },
            body: JSON.stringify({
                title,
                description
            })
        });

        if (!response.ok) {
            const data = await response.json();
            throw new Error(data.message || 'Có lỗi xảy ra khi cập nhật công việc');
        }

        showToast('Cập nhật công việc thành công');
        const modal = bootstrap.Modal.getInstance(document.getElementById('editTaskModal'));
        modal.hide();
        setTimeout(() => window.location.reload(), 1000);
    } catch (error) {
        console.error('Error:', error);
        showToast(error.message, 'error');
    }
} 