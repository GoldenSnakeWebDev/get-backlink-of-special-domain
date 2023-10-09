import './LandingPage.css';
import React, { useEffect, useState } from 'react';
import axios from 'axios';
import ReactLoading from 'react-loading';
function LandingPage () {

  const [text_userInput, setText_userInput] = useState('');
  const [backlinks, setBacklink] = useState([]);
  const [curTime, setCurTime] = useState('2023-02-03 16:00:00');
  const [link_exist, setLink_exist] = useState(false);
  const [loading, setLoading] = useState(false);

  useEffect (() => {
    const cur_time = new Date();

    const year = cur_time.getFullYear();
    const month = String(cur_time.getMonth() + 1).padStart(2, '0');
    const day = String(cur_time.getDate()).padStart(2, '0');
    const hours = String(cur_time.getHours()).padStart(2, '0');
    const minutes = String(cur_time.getMinutes()).padStart(2, '0');
    const seconds = String(cur_time.getSeconds()).padStart(2, '0');

    const formattedTime = `${year}-${month}-${day} ${hours}:${minutes}:${seconds}`;
    setCurTime(formattedTime);

    if (backlinks.length>1) {
        setLink_exist(true);
        
    }

  }, [backlinks]) 

  const handle_textarea = (event) => {
    setText_userInput(event.target.value);
  }
  const handle_get = async () => {

    setLoading(true);
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
              
              setLoading(false);
              // console.log('response>>>>>', response.data);

              setBacklink(response.data);

              console.log("backlinks>>>>", backlinks);

              console.log("first url>>>>>", response.data[1]);
              
            }).catch((error) => {
              if (error.response) {

                setLoading(false);
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

  const handle_save = () => {

  }

    return (
      <>
        <div id='main-board'>
            <input type='text' value={text_userInput} onChange={handle_textarea} placeholder='input the domain...'></input>
            <button onClick={handle_get}>GET</button>
        </div>
        
        {loading ? <div className='loading'><ReactLoading  color='grey' type='spinningBubbles' height={'20%'} width={'20%'}/> </div>:
          <div className='tableboard'>

            <table>
              <thead>
                <tr>
                  <th>No</th>
                  <th>Backlink</th>
                  <th>Cur_time</th>
                </tr>
              </thead>
              <tbody>
                {backlinks.map((link, index) => (
                  <tr key={index}>
                    <td>{(index+1).toString()}</td>
                    <td>{link.toString()}</td>
                    <td>{curTime}</td>
                  </tr>  
                ))}
              </tbody>
            </table>

            {/* {link_exist && <button className='saveButton' onClick={handle_save}>SAVE</button> } */}
            
          </div>
        }
        
      </>
    )
}

export default LandingPage;