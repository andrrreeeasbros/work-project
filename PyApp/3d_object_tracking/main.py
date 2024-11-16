import tkinter as tk
from tkinter import messagebox, filedialog
import cv2
import numpy as np
import matplotlib.pyplot as plt
from matplotlib.backends.backend_tkagg import FigureCanvasTkAgg
from PIL import Image, ImageTk

# Глобальные переменные для хранения координат и цветов
x_coords = np.random.random(10)
y_coords = np.random.random(10)
z_coords = np.random.random(10)
colors = ['b'] * 10  # Все точки синие по умолчанию

# Глобальные переменные для отслеживания минимальных и максимальных значений координат
x_min, x_max = 0, 1
y_min, y_max = 0, 1
z_min, z_max = 0, 1

# Инициализация детектора для человека (HOG)
hog = cv2.HOGDescriptor()
hog.setSVMDetector(cv2.HOGDescriptor_getDefaultPeopleDetector())

# Флаг для остановки видео
video_playing = False
cap = None  # Для захвата видео
video_length = 0  # Длина видео в кадрах

# Переменные для отслеживания предыдущих координат объекта
prev_head_x = None
prev_head_y = None
motion_threshold = 20  # Порог для определения движения (в пикселях)

# Функция для захвата видео и отслеживания объекта
def capture_video(video_path):
    global x_coords, y_coords, z_coords, x_min, x_max, y_min, y_max, z_min, z_max, cap, video_playing, colors, video_length  # Обновляем глобальные переменные
    global prev_head_x, prev_head_y  # Добавляем переменные для отслеживания предыдущих координат

    cap = cv2.VideoCapture(video_path)  # Открытие видеофайла

    if not cap.isOpened():
        print("Ошибка: Видео не найдено!")
        messagebox.showerror("Ошибка", "Не удается подключиться к видео.")
        return
    else:
        print(f"Видео '{video_path}' успешно подключено")

    # Получаем размер холста
    canvas_width = canvas.winfo_width()
    canvas_height = canvas.winfo_height()

    # Получаем длину видео в кадрах
    video_length = int(cap.get(cv2.CAP_PROP_FRAME_COUNT))

    # Устанавливаем начальные значения для перемотки
    scale_video.config(to=video_length - 1, sliderlength=20, length=600)

    video_playing = True  # Устанавливаем флаг видео в состояние "играет"
    while video_playing:
        ret, frame = cap.read()
        if not ret:
            print("Не удается захватить кадр.")
            break

        # Детекция людей на изображении
        boxes, weights = hog.detectMultiScale(frame, winStride=(8, 8), padding=(8, 8), scale=1.05)

        for (x, y, w, h) in boxes:
            # Нарисовать прямоугольник вокруг обнаруженного человека
            cv2.rectangle(frame, (x, y), (x + w, y + h), (0, 255, 0), 2)

            # Центр головы (верхняя часть прямоугольника)
            head_x = x + w // 2
            head_y = y

            # Проверяем, есть ли движение
            if prev_head_x is not None and prev_head_y is not None:
                # Вычисляем расстояние между предыдущими и текущими координатами
                dist = np.sqrt((head_x - prev_head_x) ** 2 + (head_y - prev_head_y) ** 2)
                if dist >= motion_threshold:
                    # Если движение превышает порог, меняем цвет на красный (объект движется)
                    color = 'r'
                else:
                    # Если нет движения, оставляем синий
                    color = 'b'
            else:
                # Первоначальная точка (нет предыдущих координат)
                color = 'b'

            # Обновляем координаты (передаем 2D координаты в 3D)
            x_coords = np.append(x_coords[1:], head_x / canvas_width)  # Нормализуем для 3D
            y_coords = np.append(y_coords[1:], head_y / canvas_height)
            z_coords = np.append(z_coords[1:], np.random.random())

            # Оставляем только те точки, которые движутся
            colors = [color] * len(x_coords)  # Меняем цвет точек в зависимости от движения

            # Обновляем минимальные и максимальные значения координат
            x_min = min(x_min, np.min(x_coords))
            x_max = max(x_max, np.max(x_coords))
            y_min = min(y_min, np.min(y_coords))
            y_max = max(y_max, np.max(y_coords))
            z_min = min(z_min, np.min(z_coords))
            z_max = max(z_max, np.max(z_coords))

            # Обновляем метки с новыми координатами
            label_coordinates.config(text=f"X: {x_coords[-1]:.2f} | Y: {y_coords[-1]:.2f} | Z: {z_coords[-1]:.2f}")

            # Обновляем 3D график
            plot_3d_coordinates()

            # Обновляем предыдущие координаты
            prev_head_x, prev_head_y = head_x, head_y

        # Масштабируем кадр, чтобы он помещался в холст
        frame_resized = cv2.resize(frame, (canvas.winfo_width(), canvas.winfo_height()))

        # Преобразуем изображение в формат для Tkinter
        frame_rgb = cv2.cvtColor(frame_resized, cv2.COLOR_BGR2RGB)
        frame_img = Image.fromarray(frame_rgb)
        frame_tk = ImageTk.PhotoImage(image=frame_img)

        # Обновляем изображение в Canvas
        canvas.create_image(0, 0, anchor=tk.NW, image=frame_tk)
        canvas.image = frame_tk  # Сохраняем ссылку на изображение, чтобы оно не исчезло

        window.update_idletasks()
        window.update()

    cap.release()

# Функция для отображения 3D графика с полосой
def plot_3d_coordinates():
    global x_coords, y_coords, z_coords, scatter_plot, ax, fig, x_min, x_max, y_min, y_max, z_min, z_max, colors

    # Если график еще не был создан, создаем его
    if scatter_plot is None:
        fig = plt.figure(figsize=(5, 4))
        ax = fig.add_subplot(111, projection='3d')
        scatter_plot = ax.scatter(x_coords, y_coords, z_coords, c=colors)
        ax.set_xlabel('X')
        ax.set_ylabel('Y')
        ax.set_zlabel('Z')

        # Устанавливаем динамические границы осей, чтобы они адаптировались под координаты
        ax.set_xlim([x_min, x_max])
        ax.set_ylim([y_min, y_max])
        ax.set_zlim([z_min, z_max])

        canvas_3d = FigureCanvasTkAgg(fig, master=frame_3d)
        canvas_3d.draw()
        canvas_3d.get_tk_widget().pack()
    else:
        scatter_plot._offsets3d = (x_coords, y_coords, z_coords)
        scatter_plot.set_color(colors)  # Обновляем цвета точек
        ax.set_xlim([x_min, x_max])
        ax.set_ylim([y_min, y_max])
        ax.set_zlim([z_min, z_max])
        fig.canvas.draw_idle()

# Функция для старта и остановки видео
def toggle_video():
    video_path = video_path_entry.get()  # Получаем путь к видео из текстового поля
    if video_path and not video_playing:
        start_button.config(text='Stop')
        capture_video(video_path)  # Запуск захвата видео
    elif video_playing:
        stop_video()

# Функция для остановки видео
def stop_video():
    global video_playing
    video_playing = False
    start_button.config(text='Continue')  # Меняем текст на кнопке на "Continue"

# Функция для продолжения видео
def continue_video():
    global video_playing
    video_playing = True
    start_button.config(text='Stop')  # Меняем текст на кнопке на "Stop"
    # Если видео не было загружено, то загружаем
    video_path = video_path_entry.get()
    capture_video(video_path)  # Продолжаем воспроизведение

# Функция для перемотки видео при изменении ползунка
def update_video_position(val):
    if cap and cap.isOpened():
        cap.set(cv2.CAP_PROP_POS_FRAMES, int(val))
        if video_playing:
            ret, frame = cap.read()
            if ret:
                # Масштабируем кадр, чтобы он помещался в холст
                frame_resized = cv2.resize(frame, (canvas.winfo_width(), canvas.winfo_height()))
                # Преобразуем изображение в формат для Tkinter
                frame_rgb = cv2.cvtColor(frame_resized, cv2.COLOR_BGR2RGB)
                frame_img = Image.fromarray(frame_rgb)
                frame_tk = ImageTk.PhotoImage(image=frame_img)
                # Обновляем изображение в Canvas
                canvas.create_image(0, 0, anchor=tk.NW, image=frame_tk)
                canvas.image = frame_tk  # Сохраняем ссылку на изображение, чтобы оно не исчезло

# Функция для выбора видеофайла через диалоговое окно
def browse_video():
    file_path = filedialog.askopenfilename(filetypes=[("Video Files", "*.mp4 *.avi *.mov *.mkv")])
    if file_path:
        video_path_entry.delete(0, tk.END)  # Очистить текущее значение
        video_path_entry.insert(0, file_path)  # Вставить выбранный путь

# Инициализация окна
window = tk.Tk()
window.title("3D Object Tracking System")

# Панель управления
frame_controls = tk.Frame(window)
frame_controls.pack(side=tk.TOP, fill=tk.X, padx=10, pady=10)

# Кнопка для старта/остановки видео
start_button = tk.Button(frame_controls, text="Start", command=toggle_video)
start_button.pack(side=tk.LEFT)

# Кнопка для выбора видеофайла
browse_button = tk.Button(frame_controls, text="Browse", command=browse_video)
browse_button.pack(side=tk.LEFT, padx=10)

# Поле для ввода пути к видеофайлу
video_path_label = tk.Label(frame_controls, text="Video Path:")
video_path_label.pack(side=tk.LEFT)
video_path_entry = tk.Entry(frame_controls, width=60)
video_path_entry.pack(side=tk.LEFT, padx=10)

# Окно видео
frame_video = tk.Frame(window)
frame_video.pack(side=tk.LEFT, padx=20, pady=10)

# Панель с ползунком для перемотки видео (над видео)
frame_slider = tk.Frame(frame_video)
frame_slider.pack(side=tk.TOP, fill=tk.X, pady=5)

# Ползунок для перемотки видео
scale_video = tk.Scale(frame_slider, from_=0, to=100, orient=tk.HORIZONTAL, length=600, command=update_video_position)
scale_video.pack(side=tk.LEFT, fill=tk.X)

canvas = tk.Canvas(frame_video, width=700, height=500)
canvas.pack(fill=tk.BOTH, expand=True)  # Подстраиваем холст под размер окна

# Таблица с координатами
frame_data = tk.Frame(window)
frame_data.pack(side=tk.BOTTOM, fill=tk.X, padx=10, pady=10)

label_coordinates = tk.Label(frame_data, text="X: -- | Y: -- | Z: --", font=("Arial", 14))
label_coordinates.pack(side=tk.LEFT, padx=10)

# Панель 3D визуализации
frame_3d = tk.Frame(window)
frame_3d.pack(side=tk.RIGHT, padx=20, pady=10)

# Статус индикатор
status_label = tk.Label(window, text="Tracking Active", fg="green", font=("Arial", 12))
status_label.pack(side=tk.BOTTOM)

# Глобальные переменные для графика
scatter_plot = None
ax = None
fig = None

# Запуск интерфейса
window.mainloop()
