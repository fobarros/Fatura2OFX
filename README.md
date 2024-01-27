# Conversor de Faturas PDF para OFX

Este projeto é uma solução .NET Core que permite converter faturas em formato PDF para arquivos OFX. O sistema identifica itens na fatura que são créditos ou débitos e os converte adequadamente para o formato OFX, facilitando a integração com outros sistemas financeiros.

## Estrutura do Projeto

O projeto está estruturado da seguinte maneira:

- `BRBPresentation`: Camada de apresentação que lida com a interação do usuário, upload de arquivos PDF e fornecimento de arquivos OFX para download.
- `Core`: Camada de lógica de negócio que lida com a conversão de faturas PDF para o formato OFX.
- `Infrastructure`: Camada que lida com operações de infraestrutura, como leitura e escrita de arquivos.

## 🚀 Começando

As instruções a seguir o ajudarão a obter uma cópia do projeto e a executá-lo em sua máquina local para fins de desenvolvimento e teste.

### 📋 Pré-requisitos

Antes de começar, certifique-se de ter o .NET Core SDK instalado em sua máquina. Você pode baixá-lo e instalá-lo a partir do [site oficial do .NET](https://dotnet.microsoft.com/download).

### 🔧 Instalação e Execução

1. Clone o repositório para sua máquina local:
    ```bash
    git clone https://github.com/fobarros/Fatura2OFX.git
    ```
2. Navegue até a pasta do projeto `BRBPresentation`:
    ```bash
    cd Fatura2OFX/BRBPresentation
    ```
3. Execute o projeto:
    ```bash
    dotnet run
    ```

A aplicação estará rodando e você poderá acessá-la através do navegador.

## ⚙️ Usando a Aplicação

1. Acesse a aplicação pelo navegador.
2. Faça o upload da sua fatura em formato PDF.
3. O sistema processará o arquivo, identificando créditos e débitos.
4. Um arquivo OFX será gerado e disponibilizado para download.
