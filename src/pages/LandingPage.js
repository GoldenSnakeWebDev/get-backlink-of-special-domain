import './LandingPage.css';
import React, { useEffect, useState } from 'react';
import axios from 'axios';
import ReactLoading from 'react-loading';
function LandingPage () {

  const [text_userInput, setText_userInput] = useState('');
  const [backlinks, setBacklink] = useState([]);
  const [times, setTimes] = useState([]);
  const [curTime, setCurTime] = useState('2023-02-03 16:00:00');
  const [link_exist, setLink_exist] = useState(false);
  const [loading, setLoading] = useState(false);
  const [domains, setDomains] = useState([]);

  useEffect (() => {


    axios({
      method: "post",
      url: `http://localhost:5131/api/ExistedDomain`,
    })
    .then((response) => {

      setDomains(response.data);
      
      console.log(response.data);
    }).catch((error) => {
      if (error.response) {
          alert(error);
          console.log("error~~~~~~~~~")
          console.log(error.response)
          console.log(error.response.status)
          console.log(error.response.headers)
        }
    })


    const cur_time = new Date();

    const year = cur_time.getFullYear();
    const month = String(cur_time.getMonth() + 1).padStart(2, '0');
    const day = String(cur_time.getDate()).padStart(2, '0');
    const hours = String(cur_time.getHours()).padStart(2, '0');
    const minutes = String(cur_time.getMinutes()).padStart(2, '0');
    const seconds = String(cur_time.getSeconds()).padStart(2, '0');

    const formattedTime = `${day}/${month}/${year} ${hours}:${minutes}:${seconds}`;
    setCurTime(formattedTime);

    if (backlinks.length>1) {
        setLink_exist(true);
        
    }

  }, [backlinks]) 

  const handle_textarea = (event) => {
    setText_userInput(event.target.value);
  }
  const handle_get = async () => {

    const domainContains = domains.filter(domain => domain.includes(text_userInput));

    if (domainContains.length > 0) {
      alert("This domain is already checked!");
    } else {

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
  }

  const handle_save = () => {

  }

  const formatTimes = (times) => {
    const formattedTimes = times.map(time => {
      const formattedTime = new Date(time).toLocaleString("en-GB", {
        year: "numeric",
        month: "2-digit",
        day: "2-digit",
        hour: "2-digit",
        minute: "2-digit",
        second: "2-digit"
      }).replace(/,/g, "");

      return formattedTime;

    })

    return formattedTimes;
  }

  const getExistedBacklink = (event) => {
    const selectedDomain = event.target.innerText;

    axios({
      method: "post",
      url: `http://localhost:5131/api/existedBacklink`,
      data:{'domain':selectedDomain},
      headers: {
        "Content-Type": "application/json",
      }
    })
    .then((response) => {
      
      console.log("existed domain>>>>", response.data.result_Backlink);

      setBacklink(response.data.result_Backlink);

      setTimes(formatTimes(response.data.result_time));

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

  }

    return (
      <>
        <div id='main-board'>
            <input type='text' value={text_userInput} onChange={handle_textarea} placeholder='input the domain...'></input>
            <button onClick={handle_get}>GET</button>
        </div>
        
        {loading ? <div className='loading'><ReactLoading  color='grey' type='spinningBubbles' height={'20%'} width={'20%'}/> </div>:

        <div style={{display:'flex', marginTop:'30px'}}>
          <div className='sidebar'>

            <div className='past_work'>
              <div >
                <p style={{margin:'0', fontSize:'20px', fontWeight:'500'}}>Previous work</p>
              </div>
              <div>

                <ol>

                  {domains.map((domain) => {
                    return (

                      <li style={{cursor:'pointer', textAlign:'left', marginTop:'5px', marginBottom:'5px'}} onClick={getExistedBacklink}>{domain}</li>
                    )
                  })}
                </ol>
              </div>
            </div>
          </div>
          <div className='tableboard'>

            <table>
              <thead>
                <tr>
                  <th style={{minWidth:'30px'}}>No</th>
                  <th>Backlink</th>
                  <th style={{minWidth:'50px'}}>Checked Time</th>
                </tr>
              </thead>
              <tbody>
                {backlinks.map((link, index) => (
                  <tr key={index}>
                    <td>{(index+1).toString()}</td>
                    <td>{link.toString()}</td>
                    <td>{times[index] === undefined ? curTime : times[index]}</td>
                  </tr>  
                ))}
              </tbody>
            </table>

            {/* {link_exist && <button className='saveButton' onClick={handle_save}>SAVE</button> } */}

          </div>
        </div>
          
        }
        
      </>
    )
}

export default LandingPage;