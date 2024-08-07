# Chess
Chess - шахматный fullstack проект, похожий на известные шахматные web-приложения, такие как chess.com, lichess.org и другие (скорее всего, с меньшим функционалом из-за недостатка времени и ресурсов на разработку проекта такого масштаба). Используя данное приложение пользователь сможет играть как против других пользователей, так и против искусственного интеллекта. Кроме того планируется внедрить функционал по разбору как ранее сыгранных партий, так и для анализа любых валидных пользовательских шахматных позиций. На текущий момент находится в стадии разработки. По мере готовности компонентов информация в данном файле также будет обновлятся (отдельно по каждому компоненту). Когда проект будет полностью готов для первичного релиза, добавлю информацию по установке и запуску приложения.
## Готовность приложения
На текущий момент разработана и описана архитектура приложения, а также полность готов и протестирован сервис авторизации пользователей. Все спецификации пока что предварительные, по мере разработки проекта возможны некоторые корректировки
## Архитектура приложения
Веб интерфейс планируется написать на typescript'е с использованием react'а. Работа над фронтэндом начнется, когда весь бэкэнд будет полностью готов и протестирован. Серверная часть будет разработана на основе микросервисной архитектуры с использованием языка C# версии 12. Взаимодействие между сервисами будет осуществляться в основном асинхронно посредством брокера сообщений RabbitMq через шину данных MassTransit, в некоторых случаях в синхронном режиме с помощью gRPC. В качестве прокси-сервера будет использован Yarp. Также в планах развернуть приложение в кластере kubernetes.
### Match service
Основной сервис, ответственный за управление шахматными партиями. Все взаимодействие с клиентом будет осуществляться через SignalR Core - библиотеку для двунаправленного обмена сообщениями между клиентом и сервером. Подробную спецификацию сервиса можно посмотреть [тут](specs/MatchService.pdf)
### Search service
Сервис поиска и получения шахматных партий. Подробную спецификацию сервиса можно посмотреть [тут](specs/SearchService.pdf)
### Auth service
Сервис авторизации. Спецификация [тут](specs/AuthService.pdf)
### Expiration service
Основная задача данного сервиса - периодическая проверка шахматных партий на предмет их завершения по таймингу. Спецификация [тут](specs/ExpirationService.pdf)
### AI service
Сервис будет отвечать за поиск оптимальных ходов в зависимости от заданных параметров уровня AI. Предполагается, что доступ к сервису будет осуществляться как изнутри бэкэнд-кластера при помощи gRPC запросов, так и через Web API интерфейсы. В качестве шахматного движка планируется использование [stockfish](https://github.com/official-stockfish/Stockfish) - первого в рейтинге сильнейших шахматных движков на текущий момент. Для кэширования полученных ходов также будет использован Redis. С более подробной спецификацией можете ознакомится [тут](specs/AIService.pdf)
### Events lib
Библиотека интеграционных событий для микросервисов. Описание событий [тут](specs/EventsLib.pdf)
