import React from "react";
import "./Confirm.Modal.css";

const ConfirmModal = (props) => {
    const {setIsExistedDomain, get_links} = props;
    

    const handle_Yes = () => {
        get_links();
        setIsExistedDomain(false);
    }

    const handle_No = () => {
        setIsExistedDomain(false);
    }
    return (
        <div className="modal-border">
            <div className="title">
            This domain is already checked! continue?
            </div>
            <div className="button_group">
                <button className="button" onClick={handle_Yes}>YES</button>
                <button className="button" onClick={handle_No}>NO</button>
            </div>
        </div>
    )
}

export default ConfirmModal;