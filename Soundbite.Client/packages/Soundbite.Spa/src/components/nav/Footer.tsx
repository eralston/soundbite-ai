import React, { Component } from "react";

import SpaStore from "../../store/Spa.Store";
import { ContactBtn } from "../controls/ContactBtn";

/** The contents for a footer in the app, including a set of helpful links */
export class FooterBody extends Component {
  async onLogout() {
    await SpaStore.logout();
  }

  render() {
    return (
      <ul className="nav nav-footer justify-content-center justify-content-lg-end">
        <li className="nav-item">
          <a
            href="https://soundbite.ai"
            className="nav-link"
            target="_blank"
            rel="noopener noreferrer"
            title="Soundbite.AI Website"
          >
            Soundbite.AI
            <span className="d-none d-print-inline">
              &nbsp;-&nbsp;https://Soundbite.AI
            </span>
          </a>
        </li>
        <li className="nav-item d-print-none">
          <ContactBtn textOnly={true} className="nav-link" />
        </li>
        <li className="nav-item d-print-none">
          <a
            href="https://soundbite.ai/terms-of-service/"
            className="nav-link"
            target="_blank"
            rel="noopener noreferrer"
            title="Terms of Service"
          >
            Terms of Service
          </a>
        </li>
        <li className="d-print-none">
          <button
            onClick={this.onLogout}
            className="text-dark sb-link-simple-btn nav-link"
            title="Logout"
          >
            Logout
          </button>
        </li>
      </ul>
    );
  }
}

/** A true footer for the main page that replaces itself with a standard-size padding */
export const Footer: React.FC = () => {
  return (
    <React.Fragment>
      <footer className="footer py-1 d-none d-md-block">
        <FooterBody />
      </footer>
      <div
        style={{ height: "15px" }}
        className="footer py-1 d-block d-md-none"
      ></div>
    </React.Fragment>
  );
};

/** Just the links and only visible at small size */
export const MiniFooter: React.FC = () => {
  return (
    <div className="d-block d-md-none">
      <FooterBody />
    </div>
  );
};
