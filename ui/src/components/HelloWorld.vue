<template>
  <v-container ma='4'>
    <h1 align="center">Welcome to the Quantum Open-Science Collaboration Platform</h1>
    <h3 align="center">Search for a Paper</h3>
    <!-- <v-text-field
            label="Search Bar"
            placeholder="Search"
            outlined
            append-icon="mdi-magnify"
            @input="hey()"
    ></v-text-field> -->
    <v-autocomplete
      v-model="search_text"
      :items="search_results"
      dense
      filled
      append-icon="mdi-magnify"
      label="Search"
      @input="searchSelected"
    ></v-autocomplete>
    <v-app-bar>
      <v-row no-gutters>
      <v-col
        v-for="tag in tags"
        :key="tag"
        cols="12"
        sm="6"
      >
        <v-card
          class="pa-2"
          outlined
          tile
        >
          {{ tag }}
        </v-card>
      </v-col>
    </v-row>
    </v-app-bar>
    <div
      style="padding: 25px;"
      v-if="show_results"
    >
      <h3>Search results for {{search_text}}</h3>
      <v-list v-for="result in results" :key="result">
        <a>
        <v-list-item style="border: 2px solid black" @click="showPaper">
          {{result}}
        </v-list-item>
        </a>
      </v-list>
    </div>

    <div v-if="show_article">
      <h3 align="center" style="padding: 10px">Papers</h3>
      <v-list v-for="paper in papers" :key="paper.title">
        <v-list-item>
          <div style="border: 2px solid black">
            <h4 align="center">{{ paper.title }}</h4>
            <h6> Abstract: </h6>
            <p> {{ paper.abstract }}  </p>
          </div>
        </v-list-item>
      </v-list>
      
      <h3 align="center"> Comments </h3>
      <v-list v-for="comment in comments" :key="comment.Content">
        <div style="border: 2px solid black">
          {{comment.Content}}
          <v-spacer></v-spacer>
          <v-icon
            color="black"
            @click="comment.Likes++"
          >
            mdi-thumb-up-outline
          </v-icon>
          {{comment.Likes}}
          <v-icon
            color="black"
            @click="comment.Dislikes++"
          >
            mdi-thumb-down-outline
          </v-icon>
          {{comment.Dislikes}}
          <v-spacer></v-spacer>
          <v-btn
            depressed
            @click="addComment"
          >
          Add Comment
          </v-btn>
          <v-divider></v-divider>
          <p> Replies </p>
          {{comment.replies[0].Content}}
          <v-spacer></v-spacer>
          <v-icon
            color="black"
            @click="comment.replies[0].Likes++"
          >
            mdi-thumb-up-outline
          </v-icon>
          {{comment.replies[0].Likes}}
          <v-icon
            color="black"
            @click="comment.replies[0].Dislikes++"
          >
            mdi-thumb-down-outline
          </v-icon>
          {{comment.replies[0].Dislikes}}
        </div>
      </v-list>

    <h3 align="center"> Questions </h3>
      <v-list v-for="comment in questions" :key="comment.Content">
        <div style="border: 2px solid black">
          {{comment.Content}}
          <v-spacer></v-spacer>
          <v-icon
            color="black"
            @click="comment.Likes++"
          >
            mdi-thumb-up-outline
          </v-icon>
          {{comment.Likes}}
          <v-icon
            color="black"
            @click="comment.Dislikes++"
          >
            mdi-thumb-down-outline
          </v-icon>
          {{comment.Dislikes}}
          <v-divider></v-divider>
          <p> Answers </p>
          {{comment.Answers.Content}}
          <v-spacer></v-spacer>
          <v-icon
            color="black"
            @click="comment.Answers.Likes++"
          >
            mdi-thumb-up-outline
          </v-icon>
          {{comment.Answers.Likes}}
          <v-icon
            color="black"
            @click="comment.Answers.Dislikes++"
          >
            mdi-thumb-down-outline
          </v-icon>
          {{comment.Answers.Dislikes}}
        </div>
      </v-list>
    </div>
  </v-container>
</template>

<script>
  export default {
    name: 'HelloWorld',

    data: () => ({
      search_results: [
        "On The Einstein Podolsky Rosen Paradox"
      ],
      search_text: "",
      tags: [
        "Paper",
        "Authors",
      ],
      show_results: false,
      show_article: false,
      results: [],
      papers: [
        {
          title: "Test Paper",
          Authors: "John Doe, Groucho Marx University",
          abstract: "Ullamcorper a lacus vestibulum sed arcu non. Volutpat consequat mauris nunc congue nisi vitae. Eget magna fermentum iaculis eu non diam phasellus vestibulum lorem. A pellentesque sit amet porttitor eget dolor. Et molestie ac feugiat sed lectus vestibulum mattis. Feugiat scelerisque varius morbi enim nunc. Nibh venenatis cras sed felis eget velit aliquet sagittis id. Magna fringilla urna porttitor rhoncus dolor purus non. Eu tincidunt tortor aliquam nulla facilisi. Felis eget velit aliquet sagittis id consectetur purus ut faucibus. Sit amet massa vitae tortor condimentum lacinia quis vel. Netus et malesuada fames ac turpis egestas integer. Tellus in metus vulputate eu scelerisque. Ut diam quam nulla porttitor massa id neque aliquam vestibulum.",
          PublishYear: 2021,
          PublishMonth: 12,
          Url: "https://gitlab.com/qworld/qjam/2021/-/issues/10",
          CreatedBy: 'charlie'
        }
      ],
      comments: [
        {
          Content: "Sed ut perspiciatis, unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam eaque ipsa, quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt, explicabo",
          Likes: 7,
          Dislikes: 1,
          CreatedBy: 'charlie',
          replies: [
            {
              Content: "Nemo enim ipsam voluptatem, quia voluptas sit, aspernatur aut odit aut fugit, sed quia consequuntur magni dolores eos, qui ratione voluptatem sequi nesciunt, neque porro quisquam est, qui dolorem ipsum, quia dolor sit amet consectetur adipisci[ng] velit, sed quia non numquam [do] eius modi tempora inci[di]dunt, ut labore et dolore magnam aliquam quaerat voluptatem.",
              Likes: 5,
              Dislikes: 0,
              CreatedBy: 'bob'
            }
          ]
        }
      ],
      questions: [
        {
          Content: "Mattis nunc sed blandit libero volutpat sed?",
          Likes: 3,
          Dislikes: 0,
          CreatedBy: 'charlie',
          Answers: {
                  Content: "Hac habitasse platea dictumst vestibulum rhoncus est pellentesque. Iaculis eu non diam phasellus vestibulum lorem sed risus ultricies.",
                  IsAcceptedAnswer: true,
                  Likes: 1,
                  Dislikes: 1,
                  ArticleId: 'articleId',
                  CreatedBy: 'eve'
          }
        }
      ],
      articles: [
        {
          Id: 1,
          Title: "Test Article",
          Summary: "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.",
          Tags: "test¦markdown",
          Content: "",
          Likes: 123,
          Dislikes: 1,
          Paper: 'paper',
          CreatedBy:  'alice'
        }
      ]
    }),
    methods: {
      searchSelected() {
        this.show_results = true
        this.results.push("On the Einstein Podolsky Rosen Paradox \n J.S. Bell \n Department of Physics \n 1964")
      },
      showPaper() {
        this.show_article = true
        this.show_results = false
      },
      addComment() {
        // let obj = {}
        // this.comments.push(obj)
      },
    },
    created() {
    }
  }
</script>
