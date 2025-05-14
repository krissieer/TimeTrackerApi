# Time Tracking Api
Сервис предназначен для создания клиентских приложений, реализующих интерфейс для тайм-трекинга

## Функционал

Система предоставляет возможность выполнения следующего функционала:
- Возможность регистрации и авторизации пользователя
- Возможность редактирования пользователей: изменение имени пользователя и пароля, удаление аккаунта
- Управление активностями: добавление, удаление активностей, изменение названия, архивирование
- Управление трекером времени: запуск и остановка трекера, редактирование записей отслеживания
- Вывод статистики за различные периоды
- Управление проектами: создание и удаление проектов, изменение названия, закрытие проекта, добавление и удаление пользователей и активностей

## Запуск сервиса локально
Для запуска используется Docker Docker compose. Убедитесь, что у Вас установлены данные инструменты.
1.  Склонируйте репозиторий
```
https://github.com/krissieer/TimeTrackerApi.git
```
2. Инициализируйте параметры подключения к базе данных
*  в файле .env.local:
```
POSTGRES_DB=название базы данных
POSTGRES_USER=имя пользователя базы данных
POSTGRES_PASSWORD=пароль для подключения к базе данных
DB_CONNECTION_STRING='строка подключения к базе данных'
```
* или через перемнные окружения:
```
$env:POSTGRES_USER = "имя пользователя базы данных"
$env:POSTGRES_PASSWORD = "пароль для подключения к базе данных"
$env:POSTGRES_DB = "название базы данных"
$env:DB_CONNECTION_STRING = "строка подключения к базе данных"
```
3. Для запуска контейнра используйте
* если параметры подключения к базе данных задавались в файле .env.local:
```
docker-compose -f docker-compose.local.yml --env-file .env.local up --build

```
* если параметры подключения к базе данных задавались через перемнные окружения:
```
docker compose -f docker-compose.local.yml up -d

```

## Остановка контейнера
Для остановки и удаления контейнера:
```
docker-compose -f docker-compose.local.yml down

```
Для остановки и удаления контейнера вместе с томами данными (volumes):
```
docker-compose -f docker-compose.local.yml down –v

```
## Запуск на сервере
1. Загрузите docker-образы:
```
docker pull ghcr.io/krissieer/timetrackerapi:dev
docker pull ghcr.io/krissieer/timetrackerapi-migrations:dev
```
2. Создайте docker-сеть
```
docker network create timetracker-net

```
3. Запустите контейнер с базой данных
```
docker run -d --name postgresBD --network timetracker-net -e POSTGRES_USER=имя_пользователя -e POSTGRES_PASSWORD=пароль -e POSTGRES_DB=название_БД postgres

```
4. Запустите контейнер с мигрциями
```
docker run --rm --network timetracker-net -e DB_CONNECTION_STRING="Host=postgresBD;Port=5432;Database=название_БД;Username=имя_пользователя;Password=пароль" ghcr.io/krissieer/timetrackerapi-migrations:dev

```
5. Запустите контейнер с api
```
docker run -d --name timetracking_api --network timetracker-net -e DB_CONNECTION_STRING="Host=postgresBD;Port=5432;Database=название_БД;Username=имя_пользователя;Password=пароль" -p 8080:8080 ghcr.io/krissieer/timetrackerapi:dev

```

## Остановка контейнера
Для остановки контейнера:
```
docker stop название_контейнера

```
Для удаления контейнера:
```
docker rm название_контейнера 

```
