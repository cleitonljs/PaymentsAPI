Criar imagem da api para Docker:
	docker build -t payments-api:1.0 .
	
Executar imagem:
	docker run -p 8083:8080 payments-api:1.0
	
Abrir a aplicação:
	http://localhost:8083/swagger/index.html