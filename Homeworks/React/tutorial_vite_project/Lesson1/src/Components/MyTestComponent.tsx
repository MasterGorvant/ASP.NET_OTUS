import React, { useState, useEffect, Component, ReactElement } from 'react';
import './MyTestComponent.css'

enum LoadState {
    None = 0,
    Success = 1,
    Error = 2,
  }

interface ICatFact{
    fact : string;
    length : number;
}

function GetStateName(loadState : LoadState): string {
    switch (loadState)
    {
        case LoadState.None:
            return 'Неизвестно';
        case LoadState.Error:
            return 'Ошибка';
        case LoadState.Success:
            return 'Успешно';
    }
}

const MyTestComponent = ({ url }: { url: string }) => {

  const [loadState, setLoadState] = useState<LoadState>(LoadState.None);

  const [factList, setFactList] = useState<ICatFact[]>([]);

  useEffect(() => {
    getList(url)
},[]);

const getList = async (url:string) => {
    fetch(url)
      .then((res) => res.json())
      .then((list) => {
        setLoadState(LoadState.Success);
        setFactList(list.data);
      })
      .catch((err) => {
        console.log("List ERROR ", err);
        setFactList([])
        setLoadState(LoadState.Error)
      });
  };

  // Обработчик события увеличения значения счетчика на 1
  const tryLoadSuccessfuly = () => {
    getList(url);
  };

  const tryLoadUnsuccessfuly = () => {
    getList('ololo' + url);
  };

  //TODO: обернуть в HOC
  const GetChildComponent: React.FC = () => {
    switch(loadState)
    {
        case LoadState.Error:
            return <h1 className='Error'>ERROR</h1>
        case LoadState.Success: 
            return <div>
            <h1>FACT LIST</h1>
                <ul>
                {factList.map((item, index) => 
                    <li key={index}> {item.fact} </li>
                )}
                </ul>
            </div>
            ;
        case LoadState.None:
            return <p/>;
    }
  }

  // Возвращение разметки компонента
  return (
    <div>
      <h2>Контент загруженного апи {url}:</h2>
      <p>Состояние загрузки: {GetStateName(loadState)}</p>
      <div>
            <GetChildComponent/>
      </div>
      <div>
        <button onClick={tryLoadSuccessfuly}>
            Загрузка ( успешная )
        </button>
      </div>
      <div>
        <button onClick={tryLoadUnsuccessfuly}>
            Загрузка ( неуспешная )
        </button>
      </div>
    </div>
  );
};

export default MyTestComponent;