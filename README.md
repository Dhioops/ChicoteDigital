
# Chicote Digital (WPF Verlet Physics)

Aplicativo interativo de desktop para Windows que renderiza um chicote com física em tempo real (Integração de Verlet) sobre uma janela 100% transparente.

## Controles
* **Botão Esquerdo do Mouse:** Segura o chicote pelo cabo (bloqueia os cliques no restante do Windows).
* **Scroll do Mouse (Roda):** Gira o cabo em 360º, aplicando efeito de alavanca na corda.
* **Botão Direito do Mouse:** Solta o chicote pendurado na tela e libera o cursor do mouse.
* **Tecla ESC:** Encerra o aplicativo imediatamente.

## Como Compilar
Para gerar o executável único e leve na pasta ``Executavel``:
``````powershell
dotnet publish -c Release -o ./Executavel
