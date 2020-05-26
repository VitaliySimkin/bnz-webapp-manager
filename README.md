![alt text](https://github.com/VitaliySimkin/bnz-webapp-manager/blob/master/screen.png?raw=true)

## установка
- скачайте и установите Hosting Bundle из https://dotnet.microsoft.com/download/dotnet-core/3.1
- скачиваем последний релиз - https://github.com/VitaliySimkin/bnz-webapp-manager/releases/tag/v1.1.0
- распаковываем в удобную папку [panelFolder]
- укажите путь к данной папке как "Physical Path" для корневого сайта (для того чтобы при заходе на сайт открывалась панель)
- создайте пул приложений для серверной части. .Net CLR Version - "No Managed Code". Identity - лучше LocalSystem
- добавьте приложение к сайту. Логический путь - /server/. Укажите путь к [panelFolder]/server
- DONE

API доступно по ссылке /server/swagger/index.html

## нюансы
Для оптимизации запросов перечень приложений сохраняется в кеше и обновляется раз в 5 минут. Данные про пулы приложений - обновляются постоянно


## roadmap
- общее использование RAM/CPU/Дисков
- использование CPU каждым приложении
- перечень пулов приложений
- доступ к разным серверам
- автоматическое обновление версии
- принудительное обновление перечня приложений
- учет порта на котором развернуто прилоежение
