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

// Hàm để tạo công việc mới
async function createTask(e) {
    e.preventDefault();
    
    const title = document.getElementById('taskTitle').value;
    const description = document.getElementById('taskDescription').value;
    const status = parseInt(document.getElementById('taskStatus').value);

    if (!title) {
        showToast('Vui lòng nhập tiêu đề công việc', 'error');
        return;
    }

    try {
        const response = await fetch('/api/tasks', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                title: title,
                description: description || '',
                status: status
            })
        });

        const data = await response.json();

        if (response.ok) {
            // Đóng modal
            const modal = bootstrap.Modal.getInstance(document.getElementById('addTaskModal'));
            modal.hide();
            // Reset form
            document.getElementById('taskForm').reset();
            // Hiển thị thông báo thành công
            showToast('Tạo công việc thành công', 'success');
            // Reload trang sau 1 giây
            setTimeout(() => window.location.reload(), 1000);
        } else {
            let errorMessage = 'Có lỗi xảy ra khi tạo công việc';
            if (data.errors) {
                errorMessage += ': ' + Object.values(data.errors).flat().join(', ');
            } else if (data.message) {
                errorMessage += ': ' + data.message;
            }
            showToast(errorMessage, 'error');
        }
    } catch (error) {
        console.error('Error:', error);
        showToast('Có lỗi xảy ra khi tạo công việc: ' + error.message, 'error');
    }
}

// Hàm để cập nhật trạng thái công việc
async function moveTask(id, newStatus) {
    try {
        const response = await fetch('/api/tasks/move', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
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

// Hàm để xóa công việc
async function deleteTask(id) {
    if (!confirm('Bạn có chắc chắn muốn xóa công việc này?')) {
        return;
    }

    try {
        const response = await fetch(`/api/tasks/${id}`, {
            method: 'DELETE',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (!response.ok) {
            const data = await response.json();
            throw new Error(data.message || 'Có lỗi xảy ra khi xóa công việc');
        }

        showToast('Xóa công việc thành công', 'success');
        setTimeout(() => window.location.reload(), 1000);
    } catch (error) {
        console.error('Error:', error);
        showToast(error.message, 'error');
    }
}

// Hàm hiển thị thông báo
function showToast(message, type = 'info') {
    const backgroundColor = type === 'error' ? '#dc3545' : 
                          type === 'success' ? '#198754' : 
                          '#0dcaf0';
    
    Toastify({
        text: message,
        duration: 3000,
        gravity: "top",
        position: 'right',
        style: {
            background: backgroundColor
        }
    }).showToast();
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
}); 