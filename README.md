# Pixel Arena

## Autor
João Correia, a22202506

## Introdução
Este projeto teve como objetivo a implementação de um jogo de acção multijogador, com relações cliente/servidor, sem ter login/matchmaking. Neste jogo 2D para 2 jogadores, cada um controla a base de uma de duas facções rivais, ambas visíveis no ecrã (a câmara é fixa), uma de cada lado- a facção Azul à esquerda e a facção Vermelha à direita. O objetivo do jogo é conseguir sobreviver mais tempo que o oponente, mantendo a vida da própria facção acima de 0 perante os vários inimigos que irão surgindo, e isto é conseguido ao melhorar o dano que a base dá contra inimigos ou aumentando o número de inimigos aliados que são criados progressivamente para atacarem o oponente. Para a realização deste projeto foi utilizado como base e referência o projeto MPWyzards de autoria do Professor Diogo Andrade, bem como a package de Netcode para GameObjects (NGO), que pertence ao Unity.

## Descrição Técnica
#### O Jogo
O projeto foi implementado em Unity 2D, utilizando como base o código e a estrutura do projeto MPWyzards, desenvolvido em aula com a autoria e supervisão do Professor Diogo Andrade. Em vez de se poder mover pelo mapa, neste jogo cada jogador controla uma base estática. Cada uma tem uma quantidade limitada de 5000 pontos de vida (que devem ser protegidos e preservados o melhor possível), e as bases disparam automaticamente contra quaisquer inimigos que entrem na sua ampla área de deteção.

![imagem das duas bases](https://i.postimg.cc/4d8v5dwW/1.png)

Os inimigos são NPCs com três variações que podem atacar as bases e outros inimigos de facções inimigas a curto alcance, havendo um inimigo "normal" com decente quantidade de vida e velocidades de movimento e ataque, um inimigo "fraco" que dá pouco dano e tem pouca vida mas move-se e ataca depressa, e um inimigo "tanque" que se move e ataca bastante devagar mas tem muita vida e tem alto dano.

![imagem dos tipos de inimigos](https://i.postimg.cc/c4HSXc4P/2.png)

Ambas as bases dos jogadores criam tropas da sua respetiva equipa progressivamente, e o tempo entre cada novo grupo de tropas pode ser visto através da barra amarela por baixo das barras de vida. Para além das duas facções dos jogadores, existe uma terceira facção "Selvagem" que gera inimigos periódicamente acima do ecrã em quantidades incrementais, usando um temporizador diferente do que as bases dos jogadores usam, e estes inimigos atacam qualquer uma das facções e bases dos jogadores.

![imagem dos inimigos a spawnar](https://i.postimg.cc/Cxjj2JmB/3.png)

Cada jogador tem também uma barra de Mana, visível na parte inferior do ecrã, a qual se regenera passivamente (~1 mana a cada 3 segundos) e pode acumular até um máximo de 10 de unidades, podendo estas unidades de mana ser gastas como recurso para utilizar habilidades, que são:

- 1 Mana - Aumentar o número de inimigos "fracos" que nascem em cada grupo de tropas aliadas por 1
- 3 Mana - Aumentar o número de inimigos "normais" que nascem em cada grupo de tropas aliadas por 1
- 5 Mana - Aumentar o número de inimigos "tanques" que nascem em cada grupo de tropas aliadas por 1
- 3 Mana - Aumentar o dano de cada tiro da base (tiros dão mais 15 pontos de dano por cada melhoramento);
- 3 Mana - Reduzir o tempo entre os disparos da base (tiros 15% mais rápidos por cada melhoramento);

![imagem da mana e das habilidades](https://i.postimg.cc/mgxYVKNW/4.png)

O objetivo do jogo é sobreviver mais tempo que o oponente, gerindo as habilidades de forma inteligente para preservar a vida da base e gerir o número de tropas aliadas que vão sendo criadas para atacar a base rival ou defender a própria. O jogo pode ser jogado em LAN ao utilizar uma instância do jogo para se executar como servidor, e as restantes podem-se ligar ao servidor como clientes para iniciarem uma partida. É importante notar que apenas dois jogadores se podem juntar à partida, e quaisqueres ligações adicionais poderão apenas assistir ao jogo. O jogo apenas começa quando o segundo jogador entrar no servidor do jogo, havendo um menu de espera no ecrã do primeiro enquanto o segundo jogador se connecta.

![imagem do menu de espera](https://i.postimg.cc/nc9K4nm1/5.png)

Por fim, quando um jogador perde toda a vida na sua base ou quando sai do jogo/fecha a janela, esse jogador perde e o seu adversário ganha. Aí os jogadores podem sair do servidor e voltar a criar um novo para jogar outra vez. Existe também um temporizador no canto superior esquerdo que limita o tempo que uma partida pode durar. Se nenhum jogador tiver ganho até o tempo chegar a 0, ganha o jogador que tiver mais vida.

![imagem do temporizador](https://i.postimg.cc/htkS5W2g/12.png)
![imagem do menu de vitória e derrota](https://i.postimg.cc/HLKz1dQQ/6.png)

#### Implementações
Foi utilizada a biblioteca Netcode para GameObjects (NGO) do Unity para a implementação do projeto, tendo-se seguido uma abordagem orientada para se jogar em LAN. Utilizou-se um componente Network Manager para definir e tratar de todas as mensagens entre clientes e servidor, bem como um Unity Transport para fazer a comunicação entre ambos, e um NetworkSetup para inicializar os diversos valores e métodos que vão ser corridos no servidor e nos clientes, especialmente as corrotinas StartAsClientCR, para inicializar uma sessão como um cliente que se pretende ligar a um servidor do jogo, e StartAsServerCR, para definir que essa instância irá ser o servidor onde os clientes se poderão ligar. Alguns elementos do UI foram adicionalmente desligados no servidor para desamparar a vista do jogo, embora não seja obrigatoriamente pretendido que o servidor tenha interface visual.

![imagem dos métodos de NetworkSetup](https://i.postimg.cc/mkdNV42z/7.png)   ![imagem dos métodos de NetworkSetup](https://i.postimg.cc/L4vTmtB9/8.png)

As entidades do jogo (players e inimigos) são subclasses que descendem da classe Character, a qual requer um componente de HealthSystem ao ser inicializado, garantindo que todos os Characters terão um sistema de vida associado. Character trata de inicializar os parâmetros comum a todas as personagens, como as suas animações, vida, NetworkObject (permitindo que este seja tratado como uma entidade de rede e faça parte da dinâmica de multiplayer), e Faction (ajudando a distinguir a que jogador pertence esse personagem), enquanto que Enemy apenas regista o movimento e os ataques de cada inimigo, e o PlayerTower (a base do jogador) inicializa os seus tiros e respetivas propriedades, mana, e custo das habilidades. Nota-se que a classe Character extende NetworkBehaviour, permitindo o uso de NetworkVariables.

![imagem dos parâmetros de inicialização de Character e das suas subclasses](https://i.postimg.cc/dtbTF3XQ/9.png)

Também os tiros têm a sua própria inicialização, não só guardando os seus valores em prefabs para serem depois instanciados, mas também tendo métodos de sincronização dos tiros instanciados localmente com os que foram criados no servidor. Ao disparar, o jogo envia uma mensagem ao servidor para remover o tiro local e substituí-lo pur um no servidor, fazendo uma troca praticamente discreta. Para prevenir a pequena janela entre a inicialização local do projétil e a sua troca (situação em que os inimigos demasiado perto da base não levariam dano), usou-se um LineCast para detetar se o tiro iria bater em algum obstáculo nessa "zona morta" no período de mensagens entre cliente e servidor, e se sim, procede-se com a colisão normal do tiro nessa entidade.

![imagem do código dos tiros](https://i.postimg.cc/8CKZtLKP/10.png)

Tem-se também a classe Spawner, que está encarregue de criar os vários prefabs dos inimigos. Esta classe vai aumentado o número de inimigos "Selvagens" que cria usando os Increments à medida que o jogo progride, mas trata também de instanciar os prefabs dos inimigos pertencentes às facções dos respetivos jogadores (Azuis e Vermelhos), que são criados progressivamente nas suas respetivas bases, de forma semelhante aos Selvagens mas usando um temporizador diferente. Todos os inimigos de equipas diferentes atacam-se entre si, incluindo os selvagens também, e por isso os jogadores têm de usar as suas habilidades para se defenderem não só dos inimigos da outra equipa, mas também dos selvagens. As primeiras três habilidades dos jogadores aumentam o número de cada tipo de inimigo criado em cada geração de inimigos das equipas (Fracos, Normais ou Tanques) e estes números são permanentes, portanto os jogadores podem ir aumentando a quantidade de tropas da sua equipa ao aumentar o número destas que é criado a cada geração.

![imagem dos parâmetros do spawner](https://i.postimg.cc/1txbSyFR/11.png)

Por fim temos a classe Enemy, que também extendem a classe Character tal como os jogadores, porém as entidades desta classe conseguem mexer-se pelo mapa, seguindo sempre o inimigo de uma equipa adversária mais próximo desde que esteja a menos de 100 unidades de distância. Se não houver nenhum inimigo nesse raio, então esta classe segue a base adversária mais próxima (independentemente da distância). Também a forma de ataque é diferente, dando dano quando se chegam perto o suficiente em vez de instanciar Projectiles.

![imagem dos parâmetros e escolha de alvos de Enemy](https://i.postimg.cc/7LJjj5fQ/13.png)

## Diagrama de Arquitetura de Redes
![diagrama de arquitetura de redes](https://i.postimg.cc/ZRG3YLKR/redes-diagram-drawio.png)

## Referências
Código disponibilizado e trabalhado em aula com Prof. Diogo Andrade

Projeto MPWyzards por Prof. Diogo Andrade

CodeMonkey.(2022, September 26).*COMPLETE Unity Multiplayer Tutorial (Netcode for Game Objects)* [Video].Youtube.https://www.youtube.com/watch?v=3yuBOB3VrCk&ab_channel=CodeMonkey

Diogo Andrade.(2024, May 15).*Sistemas de Redes para Jogos - Aula 15/05/2024* [Video].Youtube.https://www.youtube.com/watch?v=y7ETO57_kQY&ab_channel=DiogoAndrade

Diogo Andrade.(2024, May 22).*Sistemas de Redes para Jogos - Aula 22/05/2024* [Video].Youtube.https://www.youtube.com/watch?v=NWwIrN_hJwU&ab_channel=DiogoAndrade

Diogo Andrade.(2024, May 29).*Sistemas de Redes para Jogos - Aula 29/05/2024* [Video].Youtube.https://www.youtube.com/watch?v=FNntUfrpwWI&ab_channel=DiogoAndrade