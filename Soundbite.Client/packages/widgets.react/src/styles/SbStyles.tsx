/** @jsx jsx */
import { jsx, css, Global } from "@emotion/react";
import React, { Component } from "react";

import { GlobalTheme } from "./GlobalTheme";

const getStyles = () => {
  const colors = GlobalTheme.current.colors;
  const baseStyles = css`
  /* HTML Tags */

code {
  color: #e01a76;
}

.sb-record-btn, .sb-play-btn {
  width: 75px
}

.sb-delete-btn .btn-inner--text {
  display: none;
}

.modal-body:last-child {
  padding-bottom: 24px;
}

/* Argon and Bootstrap */

p:last-child {
  margin-bottom: 0;
}

.navbar-brand {
  font-weight: 300;
  font-size: 1.5rem;
}

@media (max-width: 360px) {
  .navbar-brand .navbar-brand-img {
    display: none;
  }
}

@media (min-width: 361px) {
  .navbar-brand .navbar-brand-img-mini {
    display: none;
  }
}

.sb-error {
  width: 100%;
}

.sb-error .alert {
  margin-left: auto;
  margin-right: auto;
}

.alert-danger-contents {
  background-color: ${colors.neutrals.foreground};
  border: 4px solid transparent;
  border-radius: 4px;
  color: ${colors.neutrals.n900};
  padding: 4px;
  overflow: hidden;
}

.alert-danger-contents p:first-of-type {
  margin-bottom: 0;
}

.alert-danger-contents h3 {
  padding-bottom: 0;
  margin-bottom: 0;
}

.sb-clickable {
  cursor: pointer;
}

.sb-card {
  padding: 1rem;
  color: ${colors.neutrals.n600};
  background-color: ${colors.neutrals.midground};
  border-radius: 0.375rem;
  border: 1px solid ${colors.neutrals.n600};
  margin-bottom: 1rem;
}

.sb-card .sb-play-button {
  margin-bottom: 1rem;
}

.sb-inline-logo {
  max-height: 3rem;
}

.sb-default-background {
  min-height: 100%;
  top: 0;
  bottom: 0;
  left: 0;
  right: 0;
}

.sb-header-logo {
  max-width: 300px;
  width: 100%;
}

.navbar-expand .navbar-collapse {
  padding: 0 15px;
}

.btn-inner--icon i:not(.fa) {
  top: 1px;
}

@media (max-width: 768px) {
  .main-content .navbar-top {
    position: relative;
    padding-bottom: 0;
  }

  .main-content .navbar-top .container {
    padding: 0 15px;
  }
}

.sb-navigation-container {
  margin-top: -4rem;
}

.sidenav-header {
  padding-top: 1rem;
}

.dropdown-menu .dropdown-item {
  cursor: pointer;
}

.input-group-text {
  font-size: 14px !important;
}

.btn .btn-inner--icon {
  margin: 0;
}

@media (max-width: 768px) {
  .btn.sb-no-small-title .btn-inner--icon svg {
    margin: 0;
  }
}

.input-group {
  border: 1px solid ${colors.neutrals.n500};
}

.input-group .form-control,
.input-group .input-group-text {
  border: none;
}

.input-group:hover {
  border-color: ${colors.neutrals.n700};
}

.input-group:focus-within:not([aria-disabled="true"]) {
  border-color: ${colors.bootstrap.primary};
  background-color: ${colors.neutrals.foreground};
}

.input-group:focus-within:not([aria-disabled="true"]) .input-group-text {
  border-color: ${colors.bootstrap.primary};
  background-color: ${colors.neutrals.foreground};
}

.modal-header {
  padding: 1.5rem;
}

.modal-header h5 {
  color: ${colors.neutrals.min};
}

.modal-header .modal-title {
  font-size: 1.0625rem;
}

.close > span:not(.sr-only) {
  font-size: 2.25rem;
}

.modal-body {
  padding-bottom: 0;
}

.modal-body > div > .form-group:last-child,
.modal-body > .form-group:last-child {
  margin-bottom: 0;
}

.modal-footer > * {
  margin: 0;
}

.modal-footer a {
  padding-right: 0.25rem;
}

.sb-new-session-modal .close {
  display: none;
}

/* Soundbite styles */
.bg-gradient-brand {
  color: ${colors.neutrals.min};
  background: linear-gradient(
    87deg,
    ${colors.brand.second} 0,
    ${colors.brand.first} 100%
  ) !important;
}

.bg-background {
  background-color: ${colors.neutrals.background}
}

.bg-midground {
  background-color: ${colors.neutrals.midground}
}

.bg-foreground {
  background-color: ${colors.neutrals.foreground}
}

.sb-progress-background {
  background: linear-gradient(
    314deg,
    ${colors.brand.second},
    ${colors.brand.first}
  );
  background-size: 100% 100%;
  transition: all 3s;
}

.sb-progress-background-go {
  background-size: 150% 150%;
  -webkit-animation: AnimationName 6s ease infinite;
  -moz-animation: AnimationName 6s ease infinite;
  animation: AnimationName 6s ease infinite;
}

@-webkit-keyframes AnimationName {
  0% {
    background-position: 9% 0%;
  }

  50% {
    background-position: 92% 100%;
  }

  100% {
    background-position: 9% 0%;
  }
}

@-moz-keyframes AnimationName {
  0% {
    background-position: 9% 0%;
  }

  50% {
    background-position: 92% 100%;
  }

  100% {
    background-position: 9% 0%;
  }
}

@keyframes AnimationName {
  0% {
    background-position: 9% 0%;
  }

  50% {
    background-position: 92% 100%;
  }

  100% {
    background-position: 9% 0%;
  }
}

.sb-loader {
  height: 36px;
  width: 36px;
}

.sb-loader .spinner-grow {
  height: 36px;
  width: 36px;
}

.sb-loader .spinner-grow.spinner-grow-sm {
  height: 18px;
  width: 18px;
}

.sb-loader .sb-avatar-loader {
  padding-top: 4px;
}

.sb-loader .spinner-grow.sb-spinner-default {
  background-color: ${colors.bootstrap.primary};
}

.sb-empty-card-with-border {
  border-collapse: collapse;
  border-top: 1px solid rgb(233, 236, 239) !important;
}

.sb-empty-card-body img {
  max-width: 50%;
  margin-top: 2rem;
  margin-bottom: 2rem;
  opacity: 0.6;
}

@media (max-width: 768px) {
  .sb-empty-card-body img {
    max-width: 80%;
  }
}

/*Custom Width to start button stack based on current layout*/
@media (max-width: 836px) {
  .sb-stackable-btn .btn {
    margin-right: 0;
    display: block;
    float: right;
    clear: both;
    width: 100%;
  }

  .sb-stackable-btn .btn:not(last-child) {
    margin-bottom: 1em;
  }
}

.sb-event-details-sm {
  font-weight: 400;
  margin-left: 1em;
}

.sb-imagepicker img {
  width: 48%;
}

.sb-imagepicker .btn-primary {
  margin-top: 1rem;
}

.sb-imagepicker .btn-primary:first-of-type {
  margin-top: 0;
}

.sb-brand {
  max-width: 240px;
}

.sb-team-link {
  position: relative;
}

.sb-team-link .sb-team-menu {
  position: absolute;
  right: 0;
  color: ${colors.neutrals.n600};
}

.sb-team-link .sb-team-menu:hover {
  color: ${colors.neutrals.n800};
}

.navbar-vertical.navbar-expand-xs
  .navbar-nav
  > .nav-item
  > .nav-link.active
  .sb-team-menu {
  right: -8px;
}

.navbar-nav
  > .nav-item
  > .nav-link.active {
  background-color: ${colors.neutrals.n200};
}

.sb-tabs {
  list-style-type: none;
  padding-left: 0;
  font-weight: 600;
  color: white;
  position: relative;
}

.sb-tabs h2 {
  margin: 0;
}

.sb-tabs a {
  color: white;
}

.sb-tabs a:active {
  color: white;
}

.sb-tabs li {
  display: inline-block;
  margin-right: 2em;
}

.sb-tabs li a.active h2 {
  border-bottom: 2px solid ${colors.neutrals.min};
}

.sb-big-dialog {
  max-width: 100%;
  margin: 0.5rem;
}

@media (min-width: 1200px) {
  .sb-big-dialog {
    margin: 1.75rem auto;
    max-width: 1200px;
  }
}

.sb-cover-dialog {
  max-width: 100%;
  margin: 0;
}

.sb-cover-dialog .modal-header {
  padding: 0.5rem 1.5rem;
}

.sb-cover-dialog .modal-header .close {
  display: none;
}

.sb-cover-dialog .modal-content video {
  max-height: 75vh;
}

.sb-cover-dialog .modal-body {
  max-width: 100%;
  margin: 0;
  padding: 0;
}

.sb-cover-dialog .sb-cover-modal-body {
  padding: 0 24px;
}

@media (max-width: 1200px) {

  .sb-cover-dialog .modal-content {
    border: 0;
    border-radius: 0;
    border-image: none;
    min-height: 100vh;
  }

  .sb-cover-dialog .modal-header {
    border: 0;
    border-radius: 0;
    border-image: none;
  }
}

@media (min-width: 1200px) {
  .sb-cover-dialog {
    margin: 0.5rem auto;
    max-width: 1200px;
  }
}

select.sb-input-group-height-fix:not([size]):not([multiple]) {
  height: calc(2.25rem + 10px);
}

.sb-select-description {
  font-size: 14px;
  margin-top: 12px;
  min-height: 21px;
}

.sb-modal-header-subtitle {
  font-weight: 300;
  font-size: 14px;
  display: block;
  line-height: 1.5;
}

.sb-meeting-title {
  display: block;
}

.sb-play-button,
.sb-record-button {
  color: ${colors.bootstrap.primary};
  height: 40px;
}

.sb-vertical-center {
  display: flex;
  align-items: center;
}

.sb-teamplayer .sb-play-button,
.sb-teamplayer .sb-record-button {
  position: absolute;
  right: calc(50% - 30px);
  top: -8px;
}

.sb-recorder {
  min-height: 50px;
  margin-bottom: 1rem;
  position: relative;
}

.sb-recorder .sb-record-button {
  position: absolute;
  right: calc(50% - 30px);
}

.sb-recorder canvas {
  border-bottom: 1px solid ${colors.neutrals.n200};
}

.sb-recorder .sb-sound-scrubber {
  margin-top: 44px;
  min-height: 6px;
}

.sb-recorder .sb-timer {
  position: absolute;
  right: 1rem;
  top: 6px;
  background-color: ${colors.neutrals.n100};
  color: ${colors.neutrals.min};
  border-radius: 4px;
  padding: 2px 6px;
}

.sb-recorder .sb-record-button .pb-container {
  width: 60px;
}

.sb-recorder .sb-record-reset-btn {
  position: absolute;
  right: calc(50% - 66px);
  top: 6px;
}

.sb-recorder .sb-record-download-btn {
  left: calc(50% - 60px);
  margin-right: 0;
  position: absolute;
  top: 6px;
}

.sb-recorder .sb-recording-scrubber-row {
  height: 6px;
  padding-top: 12px;
}

@media (min-width: 768px) {
  .sb-bottom-nav-item {
    bottom: 10px;
    width: 100%;
    margin-left: 0 !important;
    border-radius: 0 !important;
    background: ${colors.neutrals.midground};
  }
}

.sb-title-input-addon {
  width: 62px;
}

.sb-title-input-addon .input-group-text {
  padding: 10px 0;
}

.sb-tab-btn {
  cursor: pointer;
  background-color: ${colors.neutrals.midground};
  border: 1px solid ${colors.bootstrap.primary};
  color: ${colors.bootstrap.primary} !important;
}

.sb-tab-btn:hover, .sb-tab-btn.active:hover {
  color: ${colors.neutrals.min} !important;
  background-color: ${colors.bootstrap.primaryHigh};
  border: 1px solid ${colors.bootstrap.primaryHigh};
}

.sb-tab-btn.active {
  border: 1px solid ${colors.bootstrap.primary};
  background-color: ${colors.bootstrap.primary};
  color: ${colors.neutrals.min} !important;
}

.sb-3-tab .nav-item:first-of-type .sb-tab-btn {
  border-bottom-left-radius: 4px;
  border-top-left-radius: 4px;
  border-right: 0;
}

.sb-3-tab .nav-item:last-of-type .sb-tab-btn {
  border-bottom-right-radius: 4px;
  border-top-right-radius: 4px;
  border-left: 0;
}

@media (max-width: 1059px) {
  .sb-3-tab .nav-item {
    width: 100%;
  }

  .sb-3-tab .nav-item:first-of-type .sb-tab-btn {
    border-radius: 4px 4px 0 0;
    border-right: 1px solid ${colors.bootstrap.primary};
    border-bottom: 0;
  }

  .sb-3-tab .nav-item:last-of-type .sb-tab-btn {
    border-radius: 0 0 4px 4px;
    border-left: 1px solid ${colors.bootstrap.primary};
    border-top: 0;
  }
}

.sb-2-tab {
  margin-bottom: 1rem;
}

.sb-p-2-tab {
  padding: .75rem;
}

.sb-2-tab .nav-item:first-of-type .sb-tab-btn {
  border-bottom-left-radius: 4px;
  border-top-left-radius: 4px;
  border-right: 0;
}

.sb-2-tab .nav-item:last-of-type .sb-tab-btn {
  border-top-right-radius: 4px;
  border-bottom-right-radius: 4px;
  border-left: 0;
} 

.sb-record-and-play .tab-content {
  min-height: 4.5rem;
}

.sb-record-and-play[aria-disabled="true"] {
  -webkit-filter: grayscale(100%);
  -moz-filter: grayscale(100%);
  -ms-filter: grayscale(100%);
  filter: grayscale(100%);
  opacity: 0.5;
}

.sb-uploader {
  margin-bottom: 1rem;
}

.sb-explainer-image {
  max-width: 100%;
}

.list-group-flush .list-group-item:hover {
  border-bottom-color: transparent;
}

.avatar {
  min-width: 48px;
  min-height: 48px;
}

.avatar img {
  height: 100%;
}

.rdt .form-control {
  border-radius: 0;
}

.rdtPicker td.rdtActive,
.rdtPicker td.rdtActive:hover {
  max-width: 26px;
  max-height: 26px;
}

/* react-select - TODO: Figure out if the styling ability in the library would be better than hacking things here */

.sb-public-card .sb-home-action {
  margin-top: 1rem;
  display: block;
}

.sb-public-card .sb-org-link {
  border: 1px solid ${colors.bootstrap.primary};
  border-radius: 0.375rem;
  color: ${colors.bootstrap.primary}
  cursor: pointer;
  cursor: pointer;
  display: block;
  margin-bottom: 1.5rem;
  padding: 0.625rem 1.25rem;
}

.sb-public-card .sb-org-link:hover {
  background-color: ${colors.bootstrap.primary};
  border: 1px solid ${colors.bootstrap.primary};
  color: ${colors.neutrals.min};
}

.sb-public-card .sb-org-link:active {
  background-color: ${colors.bootstrap.primaryHigh};
  border: 1px solid ${colors.bootstrap.primaryHigh};
}

.sb-public-card .sb-org-link .nav-link-text {
  display: table-cell;
  padding-left: 1rem;
}

.sb-public-card .sb-org-link:last-child {
  margin-bottom: 0;
}

.sb-public-card .sb-org-link .sb-org-logo {
  background-position: center;
  background-size: contain;
  background-repeat: no-repeat;
  height: 36px;
  width: 36px;
  display: table-cell;
}

table.sb-max-width-table {
  table-layout: fixed;
  width: 100%;
}

table.sb-max-width-table th,
table.sb-max-width-table td {
  overflow: hidden;
  text-overflow: ellipsis;
}

table.sb-max-width-table td:last-of-type {
  text-overflow: clip;
}

/*!
 * https://github.com/YouCanBookMe/react-datetime
 */

.rdt {
  position: relative;
}

.rdtPicker {
  display: none;
  position: absolute;
  width: 250px;
  padding: 4px;
  margin-top: 1px;
  z-index: 99999 !important;
  background: #fff;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  border: 1px solid #f9f9f9;
}

.rdtOpen .rdtPicker {
  display: block;
}

.rdtStatic .rdtPicker {
  box-shadow: none;
  position: static;
}

.rdtPicker .rdtTimeToggle {
  text-align: center;
}

.rdtPicker table {
  width: 100%;
  margin: 0;
}

.rdtPicker td,
.rdtPicker th {
  text-align: center;
  height: 28px;
}

.rdtPicker td {
  cursor: pointer;
}

.rdtPicker td.rdtDay:hover,
.rdtPicker td.rdtHour:hover,
.rdtPicker td.rdtMinute:hover,
.rdtPicker td.rdtSecond:hover,
.rdtPicker .rdtTimeToggle:hover {
  background: #eeeeee;
  cursor: pointer;
}

.rdtPicker td.rdtOld,
.rdtPicker td.rdtNew {
  color: #999999;
}

.rdtPicker td.rdtToday {
  position: relative;
}

.rdtPicker td.rdtToday:before {
  content: "";
  display: inline-block;
  border-left: 7px solid transparent;
  border-bottom: 7px solid ${colors.bootstrap.secondary};
  border-top-color: rgba(0, 0, 0, 0.2);
  position: absolute;
  bottom: 4px;
  right: 4px;
}

.rdtPicker td.rdtActive,
.rdtPicker td.rdtActive:hover {
  background-color: ${colors.bootstrap.primary} !important;
  color: #fff;
  text-shadow: 0 -1px 0 rgba(0, 0, 0, 0.25);
}

.rdtPicker td.rdtActive.rdtToday:before {
  border-bottom-color: #fff;
}

.rdtPicker td.rdtDisabled,
.rdtPicker td.rdtDisabled:hover {
  background: none;
  color: #999999;
  cursor: not-allowed;
}

.rdtPicker td span.rdtOld {
  color: #999999;
}

.rdtPicker td span.rdtDisabled,
.rdtPicker td span.rdtDisabled:hover {
  background: none;
  color: #999999;
  cursor: not-allowed;
}

.rdtPicker th {
  border-bottom: 1px solid #f9f9f9;
}

.rdtPicker .dow {
  width: 14.2857%;
  border-bottom: none;
  cursor: default;
}

.rdtPicker th.rdtSwitch {
  width: 100px;
}

.rdtPicker th.rdtNext,
.rdtPicker th.rdtPrev {
  font-size: 21px;
  vertical-align: top;
}

.rdtPrev span,
.rdtNext span {
  display: block;
  -webkit-touch-callout: none; /* iOS Safari */
  -webkit-user-select: none; /* Chrome/Safari/Opera */
  -khtml-user-select: none; /* Konqueror */
  -moz-user-select: none; /* Firefox */
  -ms-user-select: none; /* Internet Explorer/Edge */
  user-select: none;
}

.rdtPicker th.rdtDisabled,
.rdtPicker th.rdtDisabled:hover {
  background: none;
  color: #999999;
  cursor: not-allowed;
}

.rdtPicker thead tr:first-child th {
  cursor: pointer;
}

.rdtPicker thead tr:first-child th:hover {
  background: #eeeeee;
}

.rdtPicker tfoot {
  border-top: 1px solid #f9f9f9;
}

.rdtPicker button {
  border: none;
  background: none;
  cursor: pointer;
}

.rdtPicker button:hover {
  background-color: #eee;
}

.rdtPicker thead button {
  width: 100%;
  height: 100%;
}

td.rdtMonth,
td.rdtYear {
  height: 50px;
  width: 25%;
  cursor: pointer;
}

td.rdtMonth:hover,
td.rdtYear:hover {
  background: #eee;
}

.rdtCounters {
  display: inline-block;
}

.rdtCounters > div {
  float: left;
}

.rdtCounter {
  height: 100px;
}

.rdtCounter {
  width: 40px;
}

.rdtCounterSeparator {
  line-height: 100px;
}

.rdtCounter .rdtBtn {
  height: 40%;
  line-height: 40px;
  cursor: pointer;
  display: block;
  -webkit-touch-callout: none; /* iOS Safari */
  -webkit-user-select: none; /* Chrome/Safari/Opera */
  -khtml-user-select: none; /* Konqueror */
  -moz-user-select: none; /* Firefox */
  -ms-user-select: none; /* Internet Explorer/Edge */
  user-select: none;
}

.rdtCounter .rdtBtn:hover {
  background: #eee;
}

.rdtCounter .rdtCount {
  height: 20%;
  font-size: 1.2em;
}

.rdtMilli {
  vertical-align: middle;
  padding-left: 8px;
  width: 48px;
}

.rdtMilli input {
  width: 100%;
  font-size: 1.2em;
  margin-top: 37px;
}

.rdtTime td {
  cursor: default;
}

.sb-datepicker .sb-toggle {
  margin: 11px 1rem;
}

.sb-datepicker .sb-datepicker-none-label {
  margin: 11px;
}

@media (max-width: 360px) {
  .sb-datepicker .sb-date-picker,
  .sb-datepicker .sb-time-picker {
    width: 50%;
  }
}

h1 i, h2 i, h3 i, h4 i, h5 i {
  margin-right: 0.5rem;
  opacity: 0.75;
}

.sb-cell-detail {
  font-weight: lighter;
}

/* React-mic */

.sb-react-mic {
  width: 100%;
  height: 40px;
}

button.sb-link-btn {
  background-color: transparent;
  border: none;
  box-shadow: none;
  text-decoration: underline;
  color: ${colors.neutrals.max}
}

button.sb-link-btn:hover {
  text-decoration: underline;
  color: ${colors.neutrals.max}
}

button.sb-link-simple-btn {
  background-color: transparent;
  border: none;
  box-shadow: none;
  color: ${colors.neutrals.max}
}

/* Picker */

.sb-picker > div {
  box-shadow: none;
}

.sb-picker > div > div {
  box-shadow: none;
}

.sb-picker {
  font-size: 14px;
}

.sb-picker .sb-picker__control {
  background-color: ${colors.neutrals.midground};
}

.sb-picker .sb-picker__control:focus-within,
.sb-picker__control.sb-picker__control--is-focused {
  border-color: ${colors.bootstrap.primary};
  box-shadow: none;
}

.sb-picker .picker__control {
  background-color: ${colors.neutrals.n200};
}

.sb-picker .picker__control:hover {
  border-color: ${colors.neutrals.n300};
}

.sb-picker .sb-picker__option {
  cursor: pointer;
}

.sb-picker .sb-picker__indicator.sb-picker__dropdown-indicator svg {
  fill: ${colors.neutrals.n300};
}

.sb-picker .sb-picker__option.sb-picker__option--is-focused {
  background-color: ${colors.bootstrap.primaryHigh};
  color: ${colors.neutrals.min};
}

.sb-picker .sb-picker__multi-value {
  background-color: ${colors.bootstrap.primary};
  border: 1px solid ${colors.neutrals.n300};
  border-radius: 0.375rem;
  overflow: hidden;
}

.sb-picker .sb-picker__multi-value__label {
  font-size: 14px;
  color: ${colors.neutrals.min};
  background-color: ${colors.transparent};
}

.sb-picker .sb-picker__multi-value__remove {
  cursor: pointer;
  color: ${colors.neutrals.n100};
}

.sb-picker .sb-picker__multi-value__remove:hover {
  background-color: ${colors.bootstrap.danger};
  color: ${colors.neutrals.min};
}

.sb-picker.sb-picker--is-disabled .sb-picker__control {
  background-color: ${colors.neutrals.n200};
  border: 1px solid ${colors.neutrals.n500};
}

.sb-picker.sb-picker--is-disabled .sb-picker__multi-value__remove {
  display: none;
}

.sb-picker .sb-picker__indicator-separator {
  display: none;
}

.sb-picker.sb-picker--is-disabled
  .sb-picker__indicator.sb-picker__dropdown-indicator
  svg {
  fill: ${colors.neutrals.n500};
}

.sb-picker.sb-picker--is-disabled .sb-picker__multi-value {
  background-color: ${colors.neutrals.n500};
}

.sb-picker.sb-picker--is-disabled .sb-picker__multi-value__label {
  color: ${colors.neutrals.n100};
}

.sb-picker .sb-picker__menu {
  border: 1px solid ${colors.bootstrap.primary};
  background-color: ${colors.neutrals.foreground};
}

.sb-picker .sb-picker__menu:hover {
  border: 1px solid ${colors.bootstrap.primaryHigh};
}

.sb-versionNum {
  position: fixed;
  bottom:0;
  right:0;
  font-size:10px;
  margin-right: 5px;
  pointer-events: none;
}

.sb-infinite-table {
    height: calc(100vh - 214px);
}

@media (max-width: 767.98px) {
  .sb-infinite-table {
    height: calc(100vh - 277px);
  }
}

.sb-dual-column-table tr td:first-of-type {
  max-width: 60px;
  white-space: normal;
}

.sb-dual-column-table tr td:nth-of-type(2) {
  padding: 0;
}

/***********
The sb-icon-button is mostly stolen from the SbProgressButton.tsx styles and could probably be consolidated somehow...
***********/

.sb-icon-button {
  background: ${GlobalTheme.current.colors.neutrals.midground};
  border: 1px solid ${GlobalTheme.current.colors.bootstrap.primary};
  border-radius: 0.25rem;
  color: currentColor;
  cursor: pointer;
  padding: 0.7em 1em;
  text-decoration: none;
  text-align: center;
  height: 40px;
  width: 40px;
  -webkit-tap-highlight-color: transparent;
  outline: none;
  transition: background-color 0.3s, width 0.3s, border-width 0.3s,
  border-color 0.3s, border-radius 0.3s;
  position: relative;  
  margin-left:5px;
}

.sb-icon-button:first-child {
  margin-left:0px;
}

.sb-icon-button span {
  width: 100%;
  height: 100%;
  transition: all 0.15s;
  display: inherit;
  transition: opacity 0.3s 0.1s;
  font-size: 1.4em;
  font-weight: 100;
  position: absolute;
  top: 3px;
  left: calc(50% - 9px);
}

.sb-icon-button svg {
  height: 40px;
  width: 40px;
  position: absolute;
  transform: translate(-50%, -50%);
  pointer-events: none;
}

.sb-icon-button svg path {
  opacity: 0;
  fill: none;
}

.sb-icon-button .svg-inline--fa {
  height: 1rem;
  width: 0.875em;
  overflow: visible;
}

.sb-icon-button svg.svg-inline--fa path {
  opacity: 1;
  fill: ${GlobalTheme.current.colors.bootstrap.primary};
}

.sb-icon-button:hover {
  background-color: ${GlobalTheme.current.colors.bootstrap.primaryHigh};
  border-color: ${GlobalTheme.current.colors.bootstrap.primaryHigh};
  color: white;
}

.sb-icon-button:hover svg.svg-inline--fa path {
  fill: ${GlobalTheme.current.colors.neutrals.min};
}

.sb-icon-button svg.svg-inline--fa {
  height: 20px;
  width: 20px;
  left: 10px;
  top: 15px;
}

.text-truncate-fade {
  position: relative;
  display: -webkit-box;
  -webkit-line-clamp: 1;
  -webkit-box-orient: vertical;
  overflow: hidden;
  text-overflow: ellipsis;
  line-height: 1.4em; /* Adjust this value according to your design */
  max-height: 1.4em; /* (number of lines) * (line-height value) */
}

.text-truncate-fade::after {
  content: '';
  position: absolute;
  right: 0;
  bottom: 0;
  width: 4em; /* Adjust this value according to your design */
  height: 1.4em; /* Should be the same as the line-height value */
  background: linear-gradient(to right, ${GlobalTheme.current.colors.neutrals.min}00, ${GlobalTheme.current.colors.neutrals.min}FF 75%); /* Replace the rgba() values with your background color */
  pointer-events: none;
}

video {
  background: linear-gradient(to right,${GlobalTheme.current.colors.neutrals.midground} 0%,${GlobalTheme.current.colors.neutrals.background} 50%,${GlobalTheme.current.colors.neutrals.midground} 100%);
  max-height: 100vh;
  object-fit: contain;
  width: 100%;
}

video[poster] {
  object-fit: scale-down;
}

`;
  return baseStyles;
};

/**
 * Custom Styles from Soundbite
 * */
export class SbStyles extends Component {
  static instance?: SbStyles;
  static hasRendered: boolean = false;
  constructor(props: any) {
    super(props);
    if (SbStyles.instance !== undefined) {
      console.warn(
        "More than one instance of SbStyles detected; for performance reasons, it's best to include only one instance"
      );
    }
    SbStyles.instance = this;
  }
  render() {
    if (SbStyles.hasRendered) {
      console.warn(
        "More than one render of SbStyles detected; for performance reasons, it's best to place this at the highest level of the DOM such that it is never re-rendered"
      );
    }
    SbStyles.hasRendered = true;
    // If this is the live instance, render
    if (this === SbStyles.instance) {
      const styles = getStyles();
      return <Global styles={styles} />;
    } else return <React.Fragment />;
  }
}
