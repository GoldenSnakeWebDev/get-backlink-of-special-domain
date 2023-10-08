import './LandingPage.css';
import React, { useState } from 'react';
import axios from 'axios';

function LandingPage () {

  const [text_userInput, setText_userInput] = useState('')
  const handle_textarea = (event) => {
    setText_userInput(event.target.value);
  }
    const handle_test = async () => {
        // alert('okay');
        const text = 'hello world';

        console.log('text>>>>', text );

        try {
            axios({
                method: "post",
                url: `http://localhost:5131/api/Links`,
                data:{'domain':text_userInput},
                headers: {
                  "Content-Type": "application/json",
                }
              })
              .then((response) => {
                
                console.log('response>>>>>', response.data);

                console.log("first url>>>>>", response.data[1]);
                
              }).catch((error) => {
                if (error.response) {
                    alert(error);
                    console.log("error~~~~~~~~~")
                    console.log(error.response)
                    console.log(error.response.status)
                    console.log(error.response.headers)
                  }
              })
          } catch (error) {
            console.error('error:', error);
          }
    }


    return (
        <div id='main-board'>
            <input type='text' value={text_userInput} onChange={handle_textarea}></input>
            <button onClick={handle_test}>test</button>
        </div>
    )
}

export default LandingPage;