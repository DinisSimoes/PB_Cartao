# PB_Cartoes

Worker responsável por consumir mensagens da fila cartao-credito-queue, processar as informações de criação de cartões, salvar os dados no banco de dados e publicar novos eventos em fila (por exemplo, entrega-queue, que poderia ser consumida por outro worker).

## ⚙️ Lógica de criação de cartões

A geração do número do cartão é feita usando Math.Random, apenas para simplificar o exemplo.

>Em um projeto real, essa lógica seria isolada em um serviço especializado, seguindo regras específicas de negócio.

## Como rodar localmente

Garanta que as migrations dos microserviços foram executadas

 - As instruções estão descritas no README do MS PB_Clientes.

Associe o pacote PB_Common

 - Siga os mesmos passos descritos no README do projeto PB_Clientes para adicionar o pacote local.

Execute a solução

 - Após configurar o ambiente e as dependências, basta rodar a sln normalmente.
