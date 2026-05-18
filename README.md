# Trigger AWS Lambda com S3 Events (.NET 10)

[![.NET Version](https://shields.io)](https://microsoft.com)
[![AWS Lambda](https://shields.io)](https://amazon.com)
[![Amazon S3](https://shields.io)](https://amazon.com)

Este projeto demonstra como implementar uma arquitetura orientada a eventos (Event-Driven) utilizando **.NET 10** e **AWS Lambda**. O objetivo principal é capturar eventos assíncronos de upload no **Amazon S3** para realizar o processamento escalável de imagens e conversão automatizada de thumbnails.

---

## 📌 Arquitetura do Projeto

A solução foi estruturada para separar a camada de API responsável pelo upload da camada serverless que processa os arquivos em segundo plano:

1. **MediaAPI**: Uma Web API em .NET 10 que expõe um endpoint para receber imagens e enviá-las para um bucket de origem no Amazon S3. A documentação interativa desta API utiliza o **Scalar**.
2. **Amazon S3 (Bucket)**: Armazena o arquivo original e dispara uma notificação de evento (`s3:ObjectCreated:*`) assim que o upload é concluído.
3. **AWS Lambda (ThumbnailConverter)**: Uma função Serverless em .NET 10 que é acordada pelo evento do S3, faz o download da imagem original, gera a versão em formato de miniatura (thumbnail) e salva o resultado de volta no S3.

---

## 🛠️ Tecnologias e Dependências

* **Plataforma:** .NET 10 (C# 14)
* **Documentação de API:** Scalar (`Scalar.AspNetCore`)
* **SDK Cloud:** AWS SDK for .NET
* **Eventos:** `Amazon.Lambda.S3Events`
* **Processamento de Imagem:** SixLabors.ImageSharp

---

## 📂 Estrutura da Solução

```text
├── MediaAPI/                  # Web API para upload de arquivos com interface Scalar
├── ThumbnailConverter/        # Função AWS Lambda disparada pelo evento do S3
└── TriggerLambdaWithS3.slnx   # Arquivo de solução formatado no novo padrão do VS
```

---

## 🚀 Pré-requisitos

Antes de rodar o projeto, certifique-se de ter instalado:

1. [.NET 10 SDK](https://microsoft.com/dotnet/10.0)
2. [AWS CLI](https://amazon.com) configurado com suas credenciais (`aws configure`)
3. Ferramentas Globais do Lambda instaladas:
   ```bash
   dotnet tool install -g Amazon.Lambda.Tools
   ```

---

## 🔧 Configuração na AWS

### 1. Criar os Buckets no Amazon S3
Crie um bucket para receber os uploads originais da API.

### 2. Implantar a Função Lambda
Navegue até a pasta da função e publique-a na AWS:
```bash
cd ThumbnailConverter
dotnet lambda deploy-function ThumbnailConverter
```
*Certifique-se de que a Execution Role da sua Lambda possua permissões de leitura/escrita (`s3:GetObject`, `s3:PutObject`) no seu bucket, além de permissão para gravar logs no CloudWatch.*

### 3. Configurar o Gatilho (Trigger) no S3
1. Acesse o Console da AWS > **Amazon S3** > Clique no seu bucket de upload.
2. Vá na aba **Properties** e role até **Event notifications** > **Create event notification**.
3. Defina um nome e escolha o tipo de evento **All object create events** (`s3:ObjectCreated:*`).
4. No destino, selecione **Lambda Function** e informe a sua função `ThumbnailConverter`.

---

## 💻 Como Executar Localmente

### Executando a API de Mídia
1. Configure as variáveis de ambiente ou o arquivo `appsettings.json` na pasta `MediaAPI` com as informações do seu bucket e região da AWS.
2. Rode o projeto:
   ```bash
   cd MediaAPI
   dotnet run
   ```
3. Acesse a interface interativa do **Scalar** pelo navegador (geralmente em `http://localhost:{porta}/scalar/v1`) para testar o endpoint de upload enviando uma imagem.

---

## 📚 Referências e Créditos

Este projeto foi desenvolvido com base no excelente artigo e tutorial prático do blog **Code with Mukesh**:

* **Tutorial Original:** [Trigger AWS Lambda with S3 Events in .NET - Powerful Event-Driven Thumbnail Creation Lambda for .NET Developers](https://codewithmukesh.com)
* **Autor:** Mukesh Murugan ([@codewithmukesh](https://github.com))

Agradecimentos ao autor pelo guia detalhado sobre integração arquitetural serverless orientada a eventos no ecossistema AWS com .NET.

---

## 📄 Licença

Este projeto está sob a licença MIT. Veja o arquivo [LICENSE](LICENSE) para mais detalhes.

---

Feito por [Guilherme Yasuda](https://github.com).
